from __future__ import annotations

import re
from collections.abc import Iterable


PHONE_PATTERN = re.compile(
    r"(?:\+?55\s*)?(?:\(?\d{2}\)?\s*)?(?:9?\d{4}[-.\s]?\d{4})"
)

WHATSAPP_PATTERN = re.compile(r"wa\.me/(\d+)|whatsapp[:/\s]+(\+?\d+)", re.IGNORECASE)


def normalize_phone(value: str | None) -> str | None:
    if not value:
        return None

    digits = re.sub(r"\D", "", value)
    if digits.startswith("55") and len(digits) > 11:
        digits = digits[2:]

    return digits or None


def extract_phone_candidates(text: str | None) -> list[str]:
    if not text:
        return []

    matches = [normalize_phone(match.group(0)) for match in PHONE_PATTERN.finditer(text)]
    return [match for match in matches if match]


def extract_whatsapp(text: str | None, hrefs: Iterable[str] | None = None) -> str | None:
    candidates: list[str] = []

    if text:
        candidates.extend(filter(None, extract_phone_candidates(text)))

    if hrefs:
        for href in hrefs:
            match = WHATSAPP_PATTERN.search(href or "")
            if not match:
                continue

            candidate = normalize_phone(match.group(1) or match.group(2))
            if candidate:
                candidates.append(candidate)

    return candidates[0] if candidates else None


def pick_primary_phone(*values: str | None) -> str | None:
    candidates = [normalize_phone(value) for value in values if value]
    candidates = [candidate for candidate in candidates if candidate]
    if not candidates:
        return None

    return sorted(candidates, key=len, reverse=True)[0]
