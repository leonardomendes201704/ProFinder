from __future__ import annotations

import asyncio
import logging
import random
import time
from collections import defaultdict
from collections.abc import Mapping
from urllib.parse import urlparse

import aiohttp
from fake_useragent import UserAgent
from tenacity import AsyncRetrying, retry_if_exception_type, stop_after_attempt, wait_exponential

from crawler.config import CrawlerSettings


DEFAULT_USER_AGENTS = [
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.0 Safari/605.1.15",
]


class HttpClient:
    def __init__(self, settings: CrawlerSettings, logger: logging.Logger) -> None:
        self._settings = settings
        self._logger = logger.getChild("http")
        self._semaphore = asyncio.Semaphore(settings.http_concurrency)
        self._session: aiohttp.ClientSession | None = None
        self._host_locks = defaultdict(asyncio.Lock)
        self._last_request_at: dict[str, float] = {}
        self._user_agent_provider = UserAgent() if settings.user_agent_rotation_enabled else None

    async def __aenter__(self) -> "HttpClient":
        timeout = aiohttp.ClientTimeout(total=self._settings.http_timeout_seconds)
        self._session = aiohttp.ClientSession(timeout=timeout)
        return self

    async def __aexit__(self, exc_type, exc, tb) -> None:
        if self._session:
            await self._session.close()

    async def fetch_text(self, url: str, headers: Mapping[str, str] | None = None) -> str:
        async for attempt in AsyncRetrying(
            stop=stop_after_attempt(self._settings.max_retries),
            wait=wait_exponential(multiplier=1, min=1, max=10),
            retry=retry_if_exception_type((aiohttp.ClientError, asyncio.TimeoutError)),
            reraise=True,
        ):
            with attempt:
                async with self._semaphore:
                    await self._respect_rate_limit(url)
                    session = self._ensure_session()
                    merged_headers = dict(headers or {})
                    merged_headers.setdefault("User-Agent", self._choose_user_agent())
                    proxy = random.choice(self._settings.proxy_list) if self._settings.proxy_list else None

                    self._logger.debug("GET %s", url)
                    async with session.get(url, headers=merged_headers, proxy=proxy) as response:
                        response.raise_for_status()
                        return await response.text()

        raise RuntimeError(f"Falha inesperada ao buscar {url}")

    def _ensure_session(self) -> aiohttp.ClientSession:
        if self._session is None:
            raise RuntimeError("HttpClient precisa ser usado dentro de 'async with'.")
        return self._session

    async def _respect_rate_limit(self, url: str) -> None:
        host = urlparse(url).netloc or "default"
        async with self._host_locks[host]:
            interval_seconds = self._settings.request_delay_ms / 1000
            last_request = self._last_request_at.get(host)
            if last_request is not None:
                elapsed = time.monotonic() - last_request
                wait_for = interval_seconds - elapsed
                if wait_for > 0:
                    await asyncio.sleep(wait_for)

            self._last_request_at[host] = time.monotonic()

    def _choose_user_agent(self) -> str:
        if self._user_agent_provider is None:
            return random.choice(DEFAULT_USER_AGENTS)

        try:
            return self._user_agent_provider.random
        except Exception:
            return random.choice(DEFAULT_USER_AGENTS)
