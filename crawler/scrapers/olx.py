from __future__ import annotations

from collections.abc import Callable
import logging
from urllib.parse import quote_plus, urljoin

from bs4 import BeautifulSoup

from crawler.config import CrawlerSettings
from crawler.core.browser import BrowserManager
from crawler.core.http_client import HttpClient
from crawler.core.queue import CrawlTask
from crawler.models.provider import Provider
from crawler.scrapers.base import BaseScraper, ScrapeResult
from crawler.utils.parser import build_search_query, normalize_whitespace
from crawler.utils.phone_extractor import extract_phone_candidates, extract_whatsapp


class OlxScraper(BaseScraper):
    site_key = "olx"
    lead_source_name = "OLX"

    def __init__(self, logger: logging.Logger) -> None:
        self._logger = logger.getChild(self.site_key)

    def seed_tasks(self, city: str, service: str, settings: CrawlerSettings) -> list[CrawlTask]:
        search_query = build_search_query(service, city)
        url = f"https://www.olx.com.br/brasil?q={quote_plus(search_query)}"
        return [CrawlTask(site_key=self.site_key, url=url, metadata={"search_query": search_query, "city": city})]

    async def scrape(
        self,
        task: CrawlTask,
        *,
        http_client: HttpClient,
        browser: BrowserManager,
        settings: CrawlerSettings,
        should_stop: Callable[[], bool] | None = None,
    ) -> ScrapeResult:
        result = await self._scrape_with_fallback(task, http_client=http_client, browser=browser, settings=settings)

        return result

    async def _scrape_with_fallback(
        self,
        task: CrawlTask,
        *,
        http_client: HttpClient,
        browser: BrowserManager,
        settings: CrawlerSettings,
    ) -> ScrapeResult:
        try:
            html = await http_client.fetch_text(task.url)
            result = self._parse_html(html, task, settings)
            if result.providers or not settings.browser_fallback_enabled:
                return result

            self._logger.info("HTTP sem resultado em %s, usando Playwright.", task.url)
        except Exception as exc:
            if not settings.browser_fallback_enabled:
                raise

            self._logger.warning("HTTP falhou em %s (%s), usando Playwright.", task.url, exc)

        html = await browser.fetch_html(task.url, scroll_count=3)
        return self._parse_html(html, task, settings)

    def _parse_html(self, html: str, task: CrawlTask, settings: CrawlerSettings) -> ScrapeResult:
        soup = BeautifulSoup(html, "html.parser")
        cards = soup.select('section a[href*="/d/"], article a[href*="/d/"], a[data-testid="listing-card-link"]')
        providers: list[Provider] = []

        for card in cards:
            name = normalize_whitespace(card.get_text(" ", strip=True))
            if not name:
                continue

            href = card.get("href")
            text = card.parent.get_text(" ", strip=True) if card.parent else name
            phones = extract_phone_candidates(text)
            providers.append(
                Provider(
                    name=name,
                    phone=phones[0] if phones else None,
                    whatsapp=extract_whatsapp(text, [href] if href else []),
                    address=normalize_whitespace(text),
                    city=task.metadata.get("city"),
                    website=None,
                    source=self.site_key,
                    source_listing_url=task.url,
                    source_details_url=urljoin(task.url, href) if href else task.url,
                    search_query=str(task.metadata["search_query"]),
                    raw_payload={"html_excerpt": text[:1000], "url": href},
                )
            )

        next_tasks: list[CrawlTask] = []
        if task.page_number < settings.max_pages_per_target:
            next_link = soup.select_one('a[rel="next"], a[data-testid="pagination-forward"], a[aria-label*="Pr"]')
            if next_link and next_link.get("href"):
                next_tasks.append(
                    CrawlTask(
                        site_key=self.site_key,
                        url=urljoin(task.url, next_link["href"]),
                        page_number=task.page_number + 1,
                        metadata=task.metadata,
                    )
                )

        return ScrapeResult(providers=providers, next_tasks=next_tasks, provider_count=len(providers))
