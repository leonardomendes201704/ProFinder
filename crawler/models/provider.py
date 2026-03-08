from __future__ import annotations

from dataclasses import dataclass, field
from typing import Any


@dataclass(slots=True)
class Provider:
    """Representa um prestador capturado por qualquer fonte."""

    name: str
    source: str
    search_query: str
    phone: str | None = None
    whatsapp: str | None = None
    address: str | None = None
    neighborhood: str | None = None
    city: str | None = None
    state: str | None = None
    website: str | None = None
    source_listing_url: str | None = None
    source_details_url: str | None = None
    external_id: str | None = None
    rating: float | None = None
    review_count: int | None = None
    raw_payload: dict[str, Any] = field(default_factory=dict)

    def to_export_dict(self) -> dict[str, Any]:
        return {
            "name": self.name,
            "phone": self.phone,
            "whatsapp": self.whatsapp,
            "address": self.address,
            "neighborhood": self.neighborhood,
            "city": self.city,
            "state": self.state,
            "website": self.website,
            "source": self.source,
            "search_query": self.search_query,
            "source_listing_url": self.source_listing_url,
            "source_details_url": self.source_details_url,
            "external_id": self.external_id,
            "rating": self.rating,
            "review_count": self.review_count,
        }
