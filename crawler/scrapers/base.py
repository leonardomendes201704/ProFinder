from __future__ import annotations

from collections.abc import Callable
from dataclasses import dataclass, field

from crawler.config import CrawlerSettings
from crawler.core.browser import BrowserManager
from crawler.core.http_client import HttpClient
from crawler.core.queue import CrawlTask
from crawler.models.provider import Provider


@dataclass(slots=True)
class ScrapeResult:
    providers: list[Provider] = field(default_factory=list)
    next_tasks: list[CrawlTask] = field(default_factory=list)
    provider_count: int = 0


class BaseScraper:
    site_key: str = ""
    lead_source_name: str = ""

    def seed_tasks(self, city: str, service: str, settings: CrawlerSettings) -> list[CrawlTask]:
        raise NotImplementedError

    async def scrape(
        self,
        task: CrawlTask,
        *,
        http_client: HttpClient,
        browser: BrowserManager,
        settings: CrawlerSettings,
        should_stop: Callable[[], bool] | None = None,
    ) -> ScrapeResult:
        raise NotImplementedError
