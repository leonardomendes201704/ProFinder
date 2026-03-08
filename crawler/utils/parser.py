from __future__ import annotations

import json
import re
import unicodedata
from collections.abc import Iterable
from dataclasses import dataclass


@dataclass(slots=True)
class AddressLocality:
    neighborhood: str | None = None
    city: str | None = None
    state: str | None = None


def normalize_whitespace(value: str | None) -> str | None:
    if value is None:
        return None

    normalized = re.sub(r"\s+", " ", value).strip()
    return normalized or None


def sanitize_extracted_text(value: str | None) -> str | None:
    normalized = normalize_whitespace(value)
    if not normalized:
        return None

    sanitized = "".join(
        ch
        for ch in normalized
        if unicodedata.category(ch) not in {"Co", "Cc", "Cf", "Cs"}
    )
    sanitized = re.sub(r"^[^\w\(\+]+", "", sanitized, flags=re.UNICODE)
    sanitized = re.sub(r"\s+", " ", sanitized).strip()
    return sanitized or None


def normalize_text_key(value: str | None) -> str:
    if not value:
        return ""

    normalized = unicodedata.normalize("NFKD", value)
    ascii_text = "".join(ch for ch in normalized if not unicodedata.combining(ch))
    ascii_text = re.sub(r"[^a-zA-Z0-9]+", "-", ascii_text.lower()).strip("-")
    return ascii_text


def build_search_query(service: str, city: str) -> str:
    return normalize_whitespace(f"{service} {city}") or f"{service} {city}"


def build_deduplication_key(name: str, normalized_phone: str | None, city: str | None) -> str:
    name_key = normalize_text_key(name)
    city_key = normalize_text_key(city)
    return f"{name_key}|{normalized_phone or city_key or 'sem-telefone'}"


def truncate(value: str | None, max_length: int) -> str | None:
    normalized = normalize_whitespace(value)
    if not normalized:
        return None

    return normalized[:max_length]


def merge_json_array(existing_json: str | None, values: Iterable[str | None]) -> str:
    items: list[str] = []

    if existing_json:
        try:
            parsed = json.loads(existing_json)
            if isinstance(parsed, list):
                items.extend(str(item).strip() for item in parsed if str(item).strip())
        except json.JSONDecodeError:
            pass

    items.extend(value.strip() for value in values if value and value.strip())

    unique_items: list[str] = []
    seen = set()

    for item in items:
        key = item.lower()
        if key in seen:
            continue

        seen.add(key)
        unique_items.append(item)

    return json.dumps(unique_items, ensure_ascii=False)


def extract_address_locality(
    address: str | None,
    *,
    fallback_neighborhood: str | None = None,
    fallback_city: str | None = None,
    fallback_state: str | None = None,
) -> AddressLocality:
    normalized_address = sanitize_extracted_text(address)
    locality = AddressLocality(
        neighborhood=normalize_whitespace(fallback_neighborhood),
        city=normalize_whitespace(fallback_city),
        state=normalize_whitespace(fallback_state.upper() if fallback_state else None),
    )

    if not normalized_address:
        return locality

    text = re.sub(r"(?:,\s*|\s+-\s*)\d{5}-?\d{3}\s*$", "", normalized_address).strip(" ,-")
    city_state_match = re.search(r",\s*(?P<city>[^,]+?)\s*-\s*(?P<state>[A-Za-z]{2})\s*$", text)

    if not city_state_match:
        return locality

    parsed_city = normalize_whitespace(city_state_match.group("city"))
    parsed_state = normalize_whitespace(city_state_match.group("state").upper())
    locality.city = parsed_city or locality.city
    locality.state = parsed_state or locality.state

    prefix = text[:city_state_match.start()].strip(" ,-")
    parsed_neighborhood: str | None = None
    if prefix:
        hyphen_parts = [part.strip(" ,-") for part in re.split(r"\s+-\s+", prefix) if part.strip(" ,-")]
        if len(hyphen_parts) >= 2:
            parsed_neighborhood = hyphen_parts[-1]
        else:
            comma_parts = [part.strip(" ,-") for part in prefix.split(",") if part.strip(" ,-")]
            if len(comma_parts) >= 2:
                parsed_neighborhood = comma_parts[-1]

    if parsed_neighborhood and re.search(r"[A-Za-zÀ-ÿ]", parsed_neighborhood):
        locality.neighborhood = normalize_whitespace(parsed_neighborhood)

    return locality
