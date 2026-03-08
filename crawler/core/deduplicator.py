from __future__ import annotations

from crawler.models.provider import Provider
from crawler.utils.parser import normalize_text_key
from crawler.utils.phone_extractor import normalize_phone


class ProviderDeduplicator:
    """Evita reprocessar o mesmo item bruto durante a execucao."""

    def __init__(self) -> None:
        self._seen_fingerprints: set[str] = set()

    def register(self, provider: Provider) -> bool:
        fingerprint = self._build_fingerprint(provider)
        if fingerprint in self._seen_fingerprints:
            return False

        self._seen_fingerprints.add(fingerprint)
        return True

    @staticmethod
    def _build_fingerprint(provider: Provider) -> str:
        return "|".join(
            [
                provider.source,
                normalize_text_key(provider.source_details_url or provider.source_listing_url),
                normalize_phone(provider.phone) or normalize_phone(provider.whatsapp) or "",
                normalize_text_key(provider.name),
            ]
        )
