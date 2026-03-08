from __future__ import annotations

import json
import re
import unicodedata
from collections.abc import Iterable


def normalize_whitespace(value: str | None) -> str | None:
    if value is None:
        return None

    normalized = re.sub(r"\s+", " ", value).strip()
    return normalized or None


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
