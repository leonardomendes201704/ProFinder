from __future__ import annotations

import logging
import re
import time
from dataclasses import dataclass

import requests

from crawler.config import CrawlerSettings
from crawler.models.provider import Provider
from crawler.utils.parser import normalize_whitespace, sanitize_extracted_text

PLACE_COORDINATE_PATTERN = re.compile(r"!3d(-?\d+(?:\.\d+)?)!4d(-?\d+(?:\.\d+)?)")
VIEWPORT_COORDINATE_PATTERN = re.compile(r"@(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)")


@dataclass(slots=True)
class GeoCoordinates:
    latitude: float | None = None
    longitude: float | None = None


class ProviderGeolocator:
    def __init__(self, settings: CrawlerSettings, logger: logging.Logger) -> None:
        self._settings = settings
        self._logger = logger.getChild("geolocation")
        self._session = requests.Session()
        self._session.headers.update(
            {
                "User-Agent": settings.geolocation_nominatim_user_agent,
                "Accept": "application/json",
            }
        )
        self._cache: dict[str, GeoCoordinates] = {}
        self._last_request_at = 0.0

    def close(self) -> None:
        self._session.close()

    def enrich(self, provider: Provider) -> Provider:
        if not self._settings.geolocation_enabled:
            return provider

        if provider.latitude is not None and provider.longitude is not None:
            return provider

        direct_coordinates = self._extract_from_urls(
            provider.source_listing_url,
            provider.raw_payload.get("place_url") if provider.raw_payload else None,
            provider.source_details_url,
            provider.raw_payload.get("details_url") if provider.raw_payload else None,
        )

        if direct_coordinates.latitude is not None and direct_coordinates.longitude is not None:
            provider.latitude = direct_coordinates.latitude
            provider.longitude = direct_coordinates.longitude
            provider.raw_payload["latitude"] = direct_coordinates.latitude
            provider.raw_payload["longitude"] = direct_coordinates.longitude
            return provider

        query = self._build_query(provider)
        if not query:
            return provider

        cache_key = query.lower()
        if cache_key in self._cache:
            cached = self._cache[cache_key]
            provider.latitude = cached.latitude
            provider.longitude = cached.longitude
            return provider

        coordinates = self._resolve_with_nominatim(query)
        self._cache[cache_key] = coordinates
        provider.latitude = coordinates.latitude
        provider.longitude = coordinates.longitude
        if coordinates.latitude is not None and coordinates.longitude is not None:
            provider.raw_payload["latitude"] = coordinates.latitude
            provider.raw_payload["longitude"] = coordinates.longitude
        return provider

    def _resolve_with_nominatim(self, query: str) -> GeoCoordinates:
        try:
            self._respect_rate_limit()
            response = self._session.get(
                f"{self._settings.geolocation_nominatim_base_url.rstrip('/')}/search",
                params={
                    "format": "jsonv2",
                    "limit": 1,
                    "q": query,
                },
                timeout=self._settings.geolocation_timeout_seconds,
            )
            response.raise_for_status()
            data = response.json()
            if not isinstance(data, list) or not data:
                return GeoCoordinates()

            first_item = data[0]
            latitude = _to_float(first_item.get("lat"))
            longitude = _to_float(first_item.get("lon"))
            return GeoCoordinates(latitude=latitude, longitude=longitude)
        except Exception as exc:
            self._logger.debug("Falha ao geocodificar '%s': %s", query, exc)
            return GeoCoordinates()

    def _respect_rate_limit(self) -> None:
        delay_seconds = max(self._settings.geolocation_request_delay_ms, 0) / 1000
        if delay_seconds <= 0:
            return

        elapsed = time.monotonic() - self._last_request_at
        if elapsed < delay_seconds:
            time.sleep(delay_seconds - elapsed)

        self._last_request_at = time.monotonic()

    def _build_query(self, provider: Provider) -> str | None:
        parts: list[str] = []
        address = sanitize_extracted_text(provider.address)
        if address:
            parts.append(address)

        if provider.city and (not address or provider.city.lower() not in address.lower()):
            parts.append(provider.city)

        if provider.state and (not address or provider.state.lower() not in address.lower()):
            parts.append(provider.state)

        if parts:
            parts.append("Brasil")

        return normalize_whitespace(", ".join(parts))

    def _extract_from_urls(self, *urls: object) -> GeoCoordinates:
        return extract_direct_coordinates(*urls)


def extract_direct_coordinates(*urls: object) -> GeoCoordinates:
    normalized_urls = [str(raw_url).strip() for raw_url in urls if raw_url and str(raw_url).strip()]

    for extractor in (_extract_place_coordinates, _extract_viewport_coordinates):
        for url in normalized_urls:
            coordinates = extractor(url)
            if coordinates.latitude is not None and coordinates.longitude is not None:
                return coordinates

    return GeoCoordinates()


def _extract_place_coordinates(url: str) -> GeoCoordinates:
    place_match = PLACE_COORDINATE_PATTERN.search(url)
    if not place_match:
        return GeoCoordinates()

    return GeoCoordinates(
        latitude=_to_float(place_match.group(1)),
        longitude=_to_float(place_match.group(2)),
    )


def _extract_viewport_coordinates(url: str) -> GeoCoordinates:
    at_match = VIEWPORT_COORDINATE_PATTERN.search(url)
    if not at_match:
        return GeoCoordinates()

    return GeoCoordinates(
        latitude=_to_float(at_match.group(1)),
        longitude=_to_float(at_match.group(2)),
    )


def _to_float(value: object) -> float | None:
    try:
        return float(str(value))
    except (TypeError, ValueError):
        return None
