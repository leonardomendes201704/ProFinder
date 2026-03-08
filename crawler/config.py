from __future__ import annotations

import argparse
import json
import os
from dataclasses import dataclass, field
from pathlib import Path

from dotenv import load_dotenv


SUPPORTED_SITES = ("google_maps", "olx", "telelistas", "guiamais")


@dataclass(slots=True)
class CrawlerSettings:
    concurrent_workers: int = 4
    http_concurrency: int = 12
    http_timeout_seconds: int = 30
    max_retries: int = 3
    request_delay_ms: int = 700
    browser_fallback_enabled: bool = True
    user_agent_rotation_enabled: bool = True
    proxy_list: list[str] = field(default_factory=list)
    max_pages_per_target: int = 50
    max_records_per_run: int = 5000
    google_maps_max_idle_scrolls: int = 8
    google_maps_scroll_pause_ms: int = 1500
    browser_headless: bool = True

    @classmethod
    def from_database(cls, settings: dict[str, str]) -> "CrawlerSettings":
        instance = cls()

        for key, value in settings.items():
            if key == "crawler.concurrent_workers":
                instance.concurrent_workers = _to_int(value, instance.concurrent_workers)
            elif key == "crawler.http_concurrency":
                instance.http_concurrency = _to_int(value, instance.http_concurrency)
            elif key == "crawler.http_timeout_seconds":
                instance.http_timeout_seconds = _to_int(value, instance.http_timeout_seconds)
            elif key == "crawler.max_retries":
                instance.max_retries = _to_int(value, instance.max_retries)
            elif key == "crawler.request_delay_ms":
                instance.request_delay_ms = _to_int(value, instance.request_delay_ms)
            elif key == "crawler.browser_fallback_enabled":
                instance.browser_fallback_enabled = _to_bool(value, instance.browser_fallback_enabled)
            elif key == "crawler.user_agent_rotation_enabled":
                instance.user_agent_rotation_enabled = _to_bool(value, instance.user_agent_rotation_enabled)
            elif key == "crawler.proxy_list_json":
                instance.proxy_list = _to_json_list(value)
            elif key == "crawler.max_pages_per_target":
                instance.max_pages_per_target = _to_int(value, instance.max_pages_per_target)
            elif key == "crawler.max_records_per_run":
                instance.max_records_per_run = _to_int(value, instance.max_records_per_run)
            elif key == "crawler.google_maps.max_idle_scrolls":
                instance.google_maps_max_idle_scrolls = _to_int(value, instance.google_maps_max_idle_scrolls)
            elif key == "crawler.google_maps.scroll_pause_ms":
                instance.google_maps_scroll_pause_ms = _to_int(value, instance.google_maps_scroll_pause_ms)
            elif key == "crawler.browser_headless":
                instance.browser_headless = _to_bool(value, instance.browser_headless)

        return instance


@dataclass(slots=True)
class RuntimeOptions:
    run_id: int | None
    city: str
    service: str
    connection_string: str
    profession_id: int | None
    region_id: int | None
    created_by: str
    export_dir: Path
    sites: list[str]
    log_level: str
    headless: bool | None
    max_records: int | None


def parse_args() -> RuntimeOptions:
    load_dotenv()

    parser = argparse.ArgumentParser(description="Crawler multi-site para captacao de prestadores.")
    parser.add_argument("--run-id", type=int, help="Id do lote criado externamente pela UI/admin.")
    parser.add_argument("--city", required=True, help="Cidade alvo, por exemplo: praia grande sp")
    parser.add_argument("--service", required=True, help="Servico/profissao alvo, por exemplo: eletricista")
    parser.add_argument(
        "--connection-string",
        default=os.getenv("PROFINDER_SQLSERVER_CONNECTION_STRING"),
        help="Connection string ODBC do SQL Server. Tambem pode vir de PROFINDER_SQLSERVER_CONNECTION_STRING.",
    )
    parser.add_argument("--profession-id", type=int, help="Id da profissao alvo no ProFinder.")
    parser.add_argument("--region-id", type=int, help="Id da regiao alvo no ProFinder.")
    parser.add_argument("--created-by", default="MultiSiteCrawler", help="Identificador do executor.")
    parser.add_argument("--export-dir", default="crawler/exports", help="Diretorio dos arquivos providers.csv e providers.json.")
    parser.add_argument("--sites", default=",".join(SUPPORTED_SITES), help="Lista de sites separados por virgula.")
    parser.add_argument("--log-level", default="INFO", help="Nivel de log.")
    parser.add_argument("--headless", choices=("true", "false"), help="Sobrescreve o headless do banco.")
    parser.add_argument("--max-records", type=int, help="Limite de registros para esta execucao.")

    args = parser.parse_args()

    if not args.connection_string:
        raise SystemExit("Informe a connection string ODBC via --connection-string ou PROFINDER_SQLSERVER_CONNECTION_STRING.")

    sites = [site.strip().lower() for site in args.sites.split(",") if site.strip()]
    invalid_sites = [site for site in sites if site not in SUPPORTED_SITES]
    if invalid_sites:
        raise SystemExit(f"Sites nao suportados: {', '.join(invalid_sites)}")

    return RuntimeOptions(
        run_id=args.run_id,
        city=args.city.strip(),
        service=args.service.strip(),
        connection_string=args.connection_string.strip(),
        profession_id=args.profession_id,
        region_id=args.region_id,
        created_by=args.created_by.strip(),
        export_dir=Path(args.export_dir),
        sites=sites,
        log_level=args.log_level.strip(),
        headless=None if args.headless is None else args.headless == "true",
        max_records=args.max_records,
    )


def _to_int(value: str, fallback: int) -> int:
    try:
        return int(str(value).strip())
    except (TypeError, ValueError):
        return fallback


def _to_bool(value: str, fallback: bool) -> bool:
    normalized = str(value).strip().lower()
    if normalized in {"1", "true", "sim", "yes"}:
        return True
    if normalized in {"0", "false", "nao", "no"}:
        return False
    return fallback


def _to_json_list(value: str) -> list[str]:
    try:
        parsed = json.loads(value)
    except json.JSONDecodeError:
        return []

    if not isinstance(parsed, list):
        return []

    return [str(item).strip() for item in parsed if str(item).strip()]
