from __future__ import annotations

import asyncio
from collections.abc import Awaitable, Callable
import logging
import os
import threading
import time
from urllib.parse import quote

from selenium import webdriver
from selenium.common.exceptions import TimeoutException
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.common.by import By
from selenium.webdriver.support import expected_conditions as ec
from selenium.webdriver.support.ui import WebDriverWait
from webdriver_manager.chrome import ChromeDriverManager

from crawler.config import CrawlerSettings
from crawler.core.browser import BrowserManager
from crawler.core.http_client import HttpClient
from crawler.core.queue import CrawlTask
from crawler.models.provider import Provider
from crawler.scrapers.base import BaseScraper, ScrapeResult
from crawler.utils.parser import build_search_query, normalize_whitespace, sanitize_extracted_text
from crawler.utils.phone_extractor import normalize_phone


class GoogleMapsScraper(BaseScraper):
    site_key = "google_maps"
    lead_source_name = "Google Maps"

    def __init__(self, logger: logging.Logger) -> None:
        self._logger = logger.getChild(self.site_key)

    def seed_tasks(self, city: str, service: str, settings: CrawlerSettings) -> list[CrawlTask]:
        search_query = build_search_query(service, city)
        url = f"https://www.google.com/maps/search/{quote(search_query)}"
        return [CrawlTask(site_key=self.site_key, url=url, mode="browser", metadata={"search_query": search_query})]

    async def scrape(
        self,
        task: CrawlTask,
        *,
        http_client: HttpClient,
        browser: BrowserManager,
        settings: CrawlerSettings,
        should_stop: Callable[[], bool] | None = None,
        on_provider: Callable[[Provider], Awaitable[None] | None] | Callable[[Provider], object] | None = None,
    ) -> ScrapeResult:
        if on_provider is None:
            providers, provider_count = await asyncio.to_thread(self._scrape_sync, task, settings, should_stop, None)
            return ScrapeResult(providers=providers, provider_count=provider_count)

        loop = asyncio.get_running_loop()
        queue: asyncio.Queue[Provider | object] = asyncio.Queue()
        sentinel = object()
        abort_event = threading.Event()

        def emit_provider(provider: Provider) -> None:
            loop.call_soon_threadsafe(queue.put_nowait, provider)

        def combined_should_stop() -> bool:
            return abort_event.is_set() or (should_stop() if should_stop else False)

        def run_sync() -> tuple[list[Provider], int]:
            try:
                return self._scrape_sync(task, settings, combined_should_stop, emit_provider)
            finally:
                loop.call_soon_threadsafe(queue.put_nowait, sentinel)

        thread_task = asyncio.create_task(asyncio.to_thread(run_sync))
        callback_error: Exception | None = None

        try:
            while True:
                item = await queue.get()
                if item is sentinel:
                    break

                await _invoke_provider_callback(on_provider, item)
        except Exception as exc:
            callback_error = exc
            abort_event.set()
        finally:
            providers, provider_count = await thread_task

        if callback_error is not None:
            raise callback_error

        return ScrapeResult(providers=providers, provider_count=provider_count)

    def _scrape_sync(
        self,
        task: CrawlTask,
        settings: CrawlerSettings,
        should_stop: Callable[[], bool] | None,
        emit_provider: Callable[[Provider], None] | None,
    ) -> tuple[list[Provider], int]:
        self._logger.info("Google Maps via Selenium iniciado (headless=%s).", settings.browser_headless)
        driver = self._create_driver(settings)
        providers: list[Provider] = []
        visited_urls: set[str] = set()
        idle_scrolls = 0
        captured_count = 0
        search_query = str(task.metadata["search_query"])

        try:
            driver.get(task.url)
            time.sleep(4)

            while idle_scrolls < settings.google_maps_max_idle_scrolls and captured_count < settings.max_records_per_run:
                if should_stop and should_stop():
                    self._logger.warning("Parada externa detectada no Google Maps.")
                    break

                cards = driver.find_elements(By.XPATH, '//a[contains(@href,"/place/")]')
                self._logger.info(
                    "Google Maps loop: cards=%s visitados=%s capturados=%s scrolls_ociosos=%s",
                    len(cards),
                    len(visited_urls),
                    captured_count,
                    idle_scrolls,
                )
                new_cards = False

                for card in cards:
                    if should_stop and should_stop():
                        self._logger.warning("Interrompendo iteracao do Google Maps por solicitacao externa.")
                        break

                    href = card.get_attribute("href")
                    if not href or href in visited_urls:
                        continue

                    visited_urls.add(href)
                    new_cards = True
                    driver.execute_script("arguments[0].scrollIntoView();", card)
                    time.sleep(1)

                    try:
                        card.click()
                    except Exception:
                        driver.execute_script("arguments[0].click();", card)

                    try:
                        WebDriverWait(driver, 12).until(ec.presence_of_element_located((By.CLASS_NAME, "DUwDvf")))
                    except TimeoutException:
                        self._logger.debug("Ignorado por timeout: %s", href)
                        continue

                    time.sleep(2)
                    provider = self._extract_provider(driver, href, search_query)
                    if provider:
                        if emit_provider is not None:
                            emit_provider(provider)
                        else:
                            providers.append(provider)

                        captured_count += 1
                        if captured_count <= 5 or captured_count % 10 == 0:
                            self._logger.info(
                                "Google Maps capturou %s lead(s). Ultimo: %s",
                                captured_count,
                                provider.name,
                            )

                if new_cards:
                    idle_scrolls = 0
                else:
                    idle_scrolls += 1
                    self._scroll_feed(driver, settings)

            self._logger.info("Google Maps finalizado com %s lead(s).", captured_count)
            return providers, captured_count
        finally:
            driver.quit()

    def _create_driver(self, settings: CrawlerSettings) -> webdriver.Chrome:
        options = Options()
        options.add_argument("--disable-blink-features=AutomationControlled")
        options.add_argument("--lang=pt-BR")
        options.add_argument("--start-maximized")
        browser_binary = os.getenv("CHROME_BIN") or os.getenv("GOOGLE_CHROME_BIN")
        driver_path = os.getenv("CHROMEDRIVER_PATH")
        if settings.browser_headless:
            options.add_argument("--headless=new")
            options.add_argument("--window-size=1920,1080")

        for argument in settings.browser_chrome_arguments:
            normalized_argument = str(argument).strip()
            if normalized_argument:
                options.add_argument(normalized_argument)

        if browser_binary:
            options.binary_location = browser_binary

        service = Service(driver_path) if driver_path else Service(ChromeDriverManager().install())
        return webdriver.Chrome(service=service, options=options)

    def _scroll_feed(self, driver: webdriver.Chrome, settings: CrawlerSettings) -> None:
        try:
            feed = WebDriverWait(driver, 12).until(ec.presence_of_element_located((By.XPATH, '//div[@role="feed"]')))
            driver.execute_script("arguments[0].scrollTop = arguments[0].scrollHeight", feed)
            time.sleep(settings.google_maps_scroll_pause_ms / 1000)
        except Exception:
            time.sleep(1)

    def _extract_provider(self, driver: webdriver.Chrome, href: str, search_query: str) -> Provider | None:
        try:
            name = normalize_whitespace(driver.find_element(By.CLASS_NAME, "DUwDvf").text)
        except Exception:
            return None

        phone = None
        address = None
        website = None
        rating = None
        review_count = None

        try:
            phone = sanitize_extracted_text(
                driver.find_element(By.XPATH, '//button[contains(@data-item-id,"phone")]').text
            )
        except Exception:
            pass

        try:
            address = sanitize_extracted_text(
                driver.find_element(By.XPATH, '//button[contains(@data-item-id,"address")]').text
            )
        except Exception:
            pass

        try:
            website = driver.find_element(By.XPATH, '//a[contains(@data-item-id,"authority")]').get_attribute("href")
        except Exception:
            pass

        try:
            rating_text = driver.find_element(By.CLASS_NAME, "MW4etd").text.strip().replace(",", ".")
            rating = float(rating_text)
        except Exception:
            pass

        try:
            reviews_text = driver.find_element(By.CLASS_NAME, "UY7F9").text
            digits = "".join(ch for ch in reviews_text if ch.isdigit())
            review_count = int(digits) if digits else None
        except Exception:
            pass

        raw_payload = {
            "place_url": href,
            "search_query": search_query,
            "name": name,
            "phone": phone,
            "address": address,
            "website": website,
            "rating": rating,
            "review_count": review_count,
        }

        return Provider(
            name=name,
            phone=phone,
            whatsapp=normalize_phone(phone),
            address=address,
            city=None,
            website=website,
            source=self.site_key,
            source_listing_url=href,
            source_details_url=href,
            rating=rating,
            review_count=review_count,
            search_query=search_query,
            raw_payload=raw_payload,
        )


async def _invoke_provider_callback(
    callback: Callable[[Provider], Awaitable[None] | None] | Callable[[Provider], object],
    provider: Provider,
) -> None:
    result = callback(provider)
    if asyncio.iscoroutine(result):
        await result
