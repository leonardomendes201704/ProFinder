from __future__ import annotations

import json
import logging
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

import pandas as pd
import pyodbc

from crawler.models.provider import Provider
from crawler.utils.parser import (
    build_deduplication_key,
    extract_address_locality,
    merge_json_array,
    sanitize_extracted_text,
    truncate,
)
from crawler.utils.phone_extractor import normalize_phone, pick_primary_phone


SITE_SOURCE_NAMES = {
    "google_maps": "Google Maps",
    "olx": "OLX",
    "telelistas": "Telelistas",
    "guiamais": "GuiaMais",
}


@dataclass(slots=True)
class PersistResult:
    action: str
    lead_id: int | None


class MssqlStorage:
    def __init__(self, connection_string: str, logger: logging.Logger) -> None:
        self._connection_string = connection_string
        self._logger = logger.getChild("storage")
        self._connection: pyodbc.Connection | None = None
        self._source_ids: dict[str, int] = {}

    def connect(self) -> None:
        self._connection = pyodbc.connect(self._connection_string, autocommit=False)
        self._source_ids = self._load_source_ids()

    def close(self) -> None:
        if self._connection:
            self._connection.close()
            self._connection = None

    def load_settings(self) -> dict[str, str]:
        cursor = self._cursor()
        rows = cursor.execute(
            """
            SELECT [Key], [Value]
              FROM prf_app_settings
             WHERE [Key] LIKE 'crawler.%'
            """
        ).fetchall()
        return {str(row[0]): str(row[1]) for row in rows}

    def is_stop_requested(self, run_id: int) -> bool:
        try:
            with pyodbc.connect(self._connection_string, autocommit=True) as connection:
                row = connection.cursor().execute(
                    """
                    SELECT Status
                      FROM prf_lead_capture_runs
                     WHERE Id = ?
                    """,
                    run_id,
                ).fetchone()
        except pyodbc.Error as exc:
            self._logger.warning("Falha ao consultar status externo de parada para o lote %s: %s", run_id, exc)
            return False

        return bool(row and str(row[0]).strip().lower() == "stopping")

    def start_run(self, search_query: str, created_by: str, sites: list[str], requested_run_id: int | None = None) -> int:
        now = self.utcnow()
        cursor = self._cursor()
        if requested_run_id is not None:
            updated_rows = cursor.execute(
                """
                UPDATE prf_lead_capture_runs
                   SET Status = ?,
                       SearchQuery = ?,
                       ErrorMessage = NULL,
                       StartedAt = ?,
                       CompletedAt = NULL,
                       UpdatedAt = ?,
                       CreatedBy = ?,
                       ItemsCaptured = 0,
                       ItemsInserted = 0,
                       ItemsUpdated = 0,
                       ItemsSkipped = 0
                 WHERE Id = ?
                   AND CaptureType = 'MultiSiteCrawler'
                """,
                "Running",
                truncate(search_query, 200),
                now,
                now,
                created_by,
                requested_run_id,
            ).rowcount

            if updated_rows == 0:
                raise RuntimeError(f"LeadCaptureRun {requested_run_id} nao encontrado para o MultiSiteCrawler.")

            self._connection.commit()
            return requested_run_id

        cursor.execute(
            """
            INSERT INTO prf_lead_capture_runs
            (
                LeadSourceId,
                CaptureType,
                Status,
                SearchQuery,
                Notes,
                CreatedBy,
                StartedAt,
                CompletedAt,
                CreatedAt,
                UpdatedAt,
                ItemsCaptured,
                ItemsInserted,
                ItemsUpdated,
                ItemsSkipped
            )
            OUTPUT INSERTED.Id
            VALUES (NULL, ?, ?, ?, ?, ?, ?, NULL, ?, ?, 0, 0, 0, 0)
            """,
            "MultiSiteCrawler",
            "Running",
            truncate(search_query, 200),
            truncate(f"Sites: {', '.join(sites)}", 1000),
            created_by,
            now,
            now,
            now,
        )
        row = cursor.fetchone()
        self._connection.commit()
        return int(row[0])

    def finalize_run(self, run_id: int, stats: dict[str, int], status: str, error_message: str | None = None) -> None:
        now = self.utcnow()
        self._cursor().execute(
            """
            UPDATE prf_lead_capture_runs
               SET Status = ?,
                   CompletedAt = ?,
                   UpdatedAt = ?,
                   ErrorMessage = ?,
                   ItemsCaptured = ?,
                   ItemsInserted = ?,
                   ItemsUpdated = ?,
                   ItemsSkipped = ?
             WHERE Id = ?
            """,
            status,
            now,
            now,
            truncate(error_message, 2000),
            stats["captured"],
            stats["inserted"],
            stats["updated"],
            stats["skipped"],
            run_id,
        )
        self._connection.commit()

    def upsert_provider(
        self,
        provider: Provider,
        *,
        run_id: int,
        profession_id: int | None,
        region_id: int | None,
    ) -> PersistResult:
        sanitized_phone = sanitize_extracted_text(provider.phone)
        sanitized_whatsapp = sanitize_extracted_text(provider.whatsapp)
        sanitized_address = sanitize_extracted_text(provider.address)
        locality = extract_address_locality(
            sanitized_address,
            fallback_neighborhood=provider.neighborhood,
            fallback_city=provider.city,
            fallback_state=provider.state,
        )
        lead_source_id = self._resolve_source_id(provider.source)
        normalized_phone = pick_primary_phone(sanitized_phone, sanitized_whatsapp)
        deduplication_key = build_deduplication_key(provider.name, normalized_phone, locality.city)
        now = self.utcnow()
        payload_json = json.dumps(provider.raw_payload, ensure_ascii=False)
        cursor = self._cursor()

        existing = cursor.execute(
            """
            SELECT TOP 1
                   Id,
                   ProfessionId,
                   SourceSitesJson,
                   SourceUrlsJson,
                   Phone,
                   WhatsApp,
                   Address,
                   Neighborhood,
                   City,
                   State,
                   Website
              FROM prf_provider_leads
             WHERE DeduplicationKey = ?
            """,
            deduplication_key,
        ).fetchone()

        current_primary_profession_id = int(existing[1]) if existing and existing[1] is not None else None
        effective_primary_profession_id = current_primary_profession_id or profession_id

        source_sites_json = merge_json_array(None if not existing else existing[2], [provider.source])
        source_urls_json = merge_json_array(
            None if not existing else existing[3],
            [provider.source_details_url, provider.source_listing_url],
        )
        source_count = len(json.loads(source_sites_json))

        if existing:
            lead_id = int(existing[0])
            cursor.execute(
                """
                UPDATE prf_provider_leads
                   SET LeadCaptureRunId = ?,
                       LeadSourceId = ?,
                       ProfessionId = ?,
                       RegionId = ?,
                       SiteKey = ?,
                       SearchQuery = ?,
                       Name = ?,
                       Phone = ?,
                       WhatsApp = ?,
                       NormalizedPhone = ?,
                       Address = ?,
                       Neighborhood = ?,
                       City = ?,
                       State = ?,
                       Website = ?,
                       SourceListingUrl = ?,
                       SourceDetailsUrl = ?,
                       ExternalId = ?,
                       ImportStatus = CASE
                           WHEN ImportStatus IN ('Imported', 'Ignored') THEN ImportStatus
                           ELSE 'Captured'
                       END,
                       SourceSitesJson = ?,
                       SourceUrlsJson = ?,
                       SourceCount = ?,
                       Rating = ?,
                       ReviewCount = ?,
                       RawPayloadJson = ?,
                       ScrapedAt = ?,
                       UpdatedAt = ?
                 WHERE Id = ?
                """,
                run_id,
                lead_source_id,
                effective_primary_profession_id,
                region_id,
                provider.source,
                truncate(provider.search_query, 250),
                truncate(provider.name, 200),
                truncate(_pick_best_contact(existing[4], sanitized_phone), 30),
                truncate(_pick_best_contact(existing[5], sanitized_whatsapp), 30),
                normalized_phone,
                truncate(_pick_best_text(existing[6], sanitized_address), 300),
                truncate(_pick_best_text(existing[7], locality.neighborhood), 120),
                truncate(_pick_best_text(existing[8], locality.city), 120),
                truncate(_pick_best_text(existing[9], locality.state), 10),
                truncate(_pick_best_text(existing[10], provider.website), 250),
                truncate(provider.source_listing_url, 500),
                truncate(provider.source_details_url or provider.source_listing_url, 500),
                truncate(provider.external_id, 120),
                source_sites_json,
                source_urls_json,
                source_count,
                provider.rating,
                provider.review_count,
                truncate(payload_json, 8000),
                now,
                now,
                lead_id,
            )
            self._ensure_provider_lead_professions(
                cursor,
                lead_id=lead_id,
                primary_profession_id=effective_primary_profession_id,
                profession_ids=[effective_primary_profession_id, profession_id],
            )
            self._connection.commit()
            return PersistResult(action="updated", lead_id=lead_id)

        cursor.execute(
            """
            INSERT INTO prf_provider_leads
            (
                LeadCaptureRunId,
                LeadSourceId,
                ProfessionId,
                RegionId,
                ImportedProfessionalId,
                SiteKey,
                SearchQuery,
                DeduplicationKey,
                Name,
                Phone,
                WhatsApp,
                NormalizedPhone,
                Address,
                Neighborhood,
                City,
                State,
                Website,
                SourceListingUrl,
                SourceDetailsUrl,
                ExternalId,
                ImportStatus,
                SourceSitesJson,
                SourceUrlsJson,
                SourceCount,
                Rating,
                ReviewCount,
                RawPayloadJson,
                ScrapedAt,
                CreatedAt,
                UpdatedAt
            )
            OUTPUT INSERTED.Id
            VALUES (?, ?, ?, ?, NULL, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, 'Captured', ?, ?, ?, ?, ?, ?, ?, ?, ?)
            """,
            run_id,
            lead_source_id,
            profession_id,
            region_id,
            provider.source,
            truncate(provider.search_query, 250),
            deduplication_key,
            truncate(provider.name, 200),
            truncate(sanitized_phone, 30),
            truncate(sanitized_whatsapp, 30),
            normalized_phone,
            truncate(sanitized_address, 300),
            truncate(locality.neighborhood, 120),
            truncate(locality.city, 120),
            truncate(locality.state, 10),
            truncate(provider.website, 250),
            truncate(provider.source_listing_url, 500),
            truncate(provider.source_details_url or provider.source_listing_url, 500),
            truncate(provider.external_id, 120),
            source_sites_json,
            source_urls_json,
            source_count,
            provider.rating,
            provider.review_count,
            truncate(payload_json, 8000),
            now,
            now,
            now,
        )
        row = cursor.fetchone()
        lead_id = int(row[0])
        self._ensure_provider_lead_professions(
            cursor,
            lead_id=lead_id,
            primary_profession_id=effective_primary_profession_id,
            profession_ids=[effective_primary_profession_id, profession_id],
        )
        self._connection.commit()
        return PersistResult(action="inserted", lead_id=lead_id)

    def export_run(self, run_id: int, export_dir: Path) -> tuple[Path, Path]:
        export_dir.mkdir(parents=True, exist_ok=True)
        rows = self._cursor().execute(
            """
            SELECT Name,
                   Phone,
                   WhatsApp,
                   Address,
                   Neighborhood,
                   City,
                   State,
                   Website,
                   SiteKey,
                   SearchQuery,
                   SourceListingUrl,
                   SourceDetailsUrl,
                   SourceCount,
                   Rating,
                   ReviewCount,
                   ScrapedAt
              FROM prf_provider_leads
             WHERE LeadCaptureRunId = ?
             ORDER BY ScrapedAt DESC, Id DESC
            """,
            run_id,
        ).fetchall()

        records = [
            {
                "name": row[0],
                "phone": row[1],
                "whatsapp": row[2],
                "address": row[3],
                "neighborhood": row[4],
                "city": row[5],
                "state": row[6],
                "website": row[7],
                "source": row[8],
                "search_query": row[9],
                "source_listing_url": row[10],
                "source_details_url": row[11],
                "source_count": row[12],
                "rating": row[13],
                "review_count": row[14],
                "scraped_at": row[15].isoformat() if row[15] else None,
            }
            for row in rows
        ]

        frame = pd.DataFrame(records)
        csv_path = export_dir / "providers.csv"
        json_path = export_dir / "providers.json"
        frame.to_csv(csv_path, index=False, encoding="utf-8-sig")
        frame.to_json(json_path, orient="records", force_ascii=False, indent=2)
        return csv_path, json_path

    def _load_source_ids(self) -> dict[str, int]:
        rows = self._cursor().execute("SELECT Id, Name FROM prf_lead_sources WHERE IsActive = 1").fetchall()
        return {str(row[1]).strip().lower(): int(row[0]) for row in rows}

    def _resolve_source_id(self, site_key: str) -> int:
        expected_name = SITE_SOURCE_NAMES.get(site_key, "Outro").lower()
        if expected_name not in self._source_ids:
            raise RuntimeError(
                f"Origem '{expected_name}' nao encontrada em prf_lead_sources. Cadastre/ative a origem no admin."
            )
        return self._source_ids[expected_name]

    def _ensure_provider_lead_professions(
        self,
        cursor: pyodbc.Cursor,
        *,
        lead_id: int,
        primary_profession_id: int | None,
        profession_ids: list[int | None],
    ) -> None:
        distinct_profession_ids = [profession_id for profession_id in dict.fromkeys(profession_ids) if profession_id is not None]

        for profession_id in distinct_profession_ids:
            cursor.execute(
                """
                IF NOT EXISTS (
                    SELECT 1
                      FROM prf_provider_lead_professions
                     WHERE ProviderLeadId = ?
                       AND ProfessionId = ?
                )
                BEGIN
                    INSERT INTO prf_provider_lead_professions
                    (
                        ProviderLeadId,
                        ProfessionId,
                        IsPrimary
                    )
                    VALUES (?, ?, ?)
                END
                """,
                lead_id,
                profession_id,
                lead_id,
                profession_id,
                1 if profession_id == primary_profession_id else 0,
            )

        if primary_profession_id is not None:
            cursor.execute(
                """
                UPDATE prf_provider_lead_professions
                   SET IsPrimary = CASE WHEN ProfessionId = ? THEN 1 ELSE 0 END
                 WHERE ProviderLeadId = ?
                """,
                primary_profession_id,
                lead_id,
            )

    def _cursor(self) -> pyodbc.Cursor:
        if self._connection is None:
            raise RuntimeError("Storage nao conectado.")
        return self._connection.cursor()

    @staticmethod
    def utcnow() -> datetime:
        return datetime.now(timezone.utc).replace(tzinfo=None)


def _pick_best_contact(existing: Any, incoming: str | None) -> str | None:
    existing_normalized = normalize_phone(str(existing)) if existing else None
    incoming_normalized = normalize_phone(incoming)

    if incoming_normalized and (not existing_normalized or len(incoming_normalized) >= len(existing_normalized)):
        return incoming

    return str(existing).strip() if existing else incoming


def _pick_best_text(existing: Any, incoming: str | None) -> str | None:
    existing_text = str(existing).strip() if existing else None
    incoming_text = incoming.strip() if incoming else None

    if incoming_text and (not existing_text or len(incoming_text) > len(existing_text)):
        return incoming_text

    return existing_text or incoming_text
