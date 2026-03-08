from __future__ import annotations

import logging

from crawler.config import CrawlerSettings


class BrowserManager:
    """Wrapper simples de Playwright para fallback em paginas dinamicas."""

    def __init__(self, settings: CrawlerSettings, logger: logging.Logger) -> None:
        self._settings = settings
        self._logger = logger.getChild("browser")

    async def fetch_html(self, url: str, wait_for_selector: str | None = None, scroll_count: int = 0) -> str:
        try:
            from playwright.async_api import async_playwright
        except ImportError as exc:  # pragma: no cover - dependencia de runtime
            raise RuntimeError("Playwright nao instalado. Execute 'playwright install'.") from exc

        self._logger.info("Playwright GET %s (headless=%s)", url, self._settings.browser_headless)

        async with async_playwright() as playwright:
            browser = await playwright.chromium.launch(headless=self._settings.browser_headless)
            page = await browser.new_page()
            try:
                await page.goto(url, wait_until="domcontentloaded", timeout=self._settings.http_timeout_seconds * 1000)
                if wait_for_selector:
                    await page.wait_for_selector(wait_for_selector, timeout=self._settings.http_timeout_seconds * 1000)

                for _ in range(scroll_count):
                    await page.mouse.wheel(0, 5000)
                    await page.wait_for_timeout(self._settings.request_delay_ms)

                return await page.content()
            finally:
                await browser.close()
