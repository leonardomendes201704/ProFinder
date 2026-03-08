from __future__ import annotations

import asyncio
import logging
import time
from dataclasses import dataclass

from crawler.config import CrawlerSettings, RuntimeOptions
from crawler.core.browser import BrowserManager
from crawler.core.deduplicator import ProviderDeduplicator
from crawler.core.http_client import HttpClient
from crawler.core.queue import AsyncTaskQueue, CrawlTask
from crawler.models.provider import Provider
from crawler.scrapers.base import BaseScraper
from crawler.scrapers.google_maps import GoogleMapsScraper
from crawler.scrapers.guiamais import GuiaMaisScraper
from crawler.scrapers.olx import OlxScraper
from crawler.scrapers.telelistas import TelelistasScraper
from crawler.storage import MssqlStorage
from crawler.utils.parser import build_search_query


@dataclass(slots=True)
class CrawlStats:
    captured: int = 0
    inserted: int = 0
    updated: int = 0
    skipped: int = 0
    failed: int = 0

    def as_dict(self) -> dict[str, int]:
        return {
            "captured": self.captured,
            "inserted": self.inserted,
            "updated": self.updated,
            "skipped": self.skipped,
            "failed": self.failed,
        }


class CrawlScheduler:
    def __init__(
        self,
        runtime: RuntimeOptions,
        settings: CrawlerSettings,
        storage: MssqlStorage,
        logger: logging.Logger,
    ) -> None:
        self._runtime = runtime
        self._settings = settings
        self._storage = storage
        self._logger = logger.getChild("scheduler")
        self._queue = AsyncTaskQueue()
        self._deduplicator = ProviderDeduplicator()
        self._stop_event = asyncio.Event()
        self._stats = CrawlStats()
        self._current_run_id: int | None = None
        self._external_stop_requested = False
        self._last_external_stop_check = 0.0
        self._last_failure_message: str | None = None
        self._scrapers: dict[str, BaseScraper] = {
            "google_maps": GoogleMapsScraper(logger),
            "olx": OlxScraper(logger),
            "telelistas": TelelistasScraper(logger),
            "guiamais": GuiaMaisScraper(logger),
        }

    async def run(self) -> tuple[int, CrawlStats]:
        search_query = build_search_query(self._runtime.service, self._runtime.city)
        self._current_run_id = self._storage.start_run(
            search_query,
            self._runtime.created_by,
            self._runtime.sites,
            requested_run_id=self._runtime.run_id)
        self._logger.info("Lote %s iniciado para '%s'.", self._current_run_id, search_query)

        try:
            async with HttpClient(self._settings, self._logger) as http_client:
                browser = BrowserManager(self._settings, self._logger)

                for task in self._seed_tasks():
                    await self._queue.put(task)

                workers = [
                    asyncio.create_task(self._worker(http_client=http_client, browser=browser))
                    for _ in range(self._settings.concurrent_workers)
                ]

                await self._queue.join()

                for _ in workers:
                    await self._queue.put(None)

                await asyncio.gather(*workers)

            final_status = "Stopped" if self._external_stop_requested else ("Failed" if self._stats.failed > 0 else "Completed")
            error_message = self._last_failure_message if final_status == "Failed" else None
            self._storage.finalize_run(self._current_run_id, self._stats.as_dict(), final_status, error_message)
            return self._current_run_id, self._stats
        except Exception as exc:
            if self._current_run_id is not None:
                self._storage.finalize_run(self._current_run_id, self._stats.as_dict(), "Failed", str(exc))
            raise

    def export(self, run_id: int) -> tuple[str, str]:
        csv_path, json_path = self._storage.export_run(run_id, self._runtime.export_dir)
        return str(csv_path), str(json_path)

    def _seed_tasks(self) -> list[CrawlTask]:
        tasks: list[CrawlTask] = []
        for site_key in self._runtime.sites:
            scraper = self._scrapers[site_key]
            tasks.extend(scraper.seed_tasks(self._runtime.city, self._runtime.service, self._settings))
        return tasks

    async def _worker(self, *, http_client: HttpClient, browser: BrowserManager) -> None:
        while True:
            task = await self._queue.get()
            if task is None:
                self._queue.task_done()
                return

            try:
                if self._stop_event.is_set():
                    continue

                if self._check_external_stop():
                    self._stop_event.set()
                    continue

                self._logger.info(
                    "Processando task site=%s modo=%s pagina=%s url=%s",
                    task.site_key,
                    task.mode,
                    task.page_number,
                    task.url,
                )
                scraper = self._scrapers[task.site_key]
                stop_callback = self._is_stop_requested
                if task.site_key == "google_maps":
                    result = await scraper.scrape(
                        task,
                        http_client=http_client,
                        browser=browser,
                        settings=self._settings,
                        should_stop=stop_callback,
                        on_provider=self._process_provider_streamed,
                    )
                else:
                    result = await scraper.scrape(
                        task,
                        http_client=http_client,
                        browser=browser,
                        settings=self._settings,
                        should_stop=stop_callback,
                    )

                self._logger.info(
                    "Task concluida site=%s pagina=%s providers=%s proximas=%s",
                    task.site_key,
                    task.page_number,
                    result.provider_count or len(result.providers),
                    len(result.next_tasks),
                )

                for provider in result.providers:
                    if self._check_external_stop():
                        self._stop_event.set()
                        break

                    self._process_provider(provider)
                    if self._should_stop():
                        self._stop_event.set()
                        break

                if not self._stop_event.is_set():
                    for next_task in result.next_tasks:
                        await self._queue.put(next_task)
            except Exception as exc:
                self._stats.failed += 1
                if self._last_failure_message is None:
                    self._last_failure_message = f"Falha ao processar task {task.site_key} pagina {task.page_number}: {exc}"
                self._logger.exception("Falha ao processar task %s", task)
            finally:
                self._queue.task_done()

    async def _process_provider_streamed(self, provider: Provider) -> None:
        self._process_provider(provider)
        if self._should_stop():
            self._stop_event.set()

    def _process_provider(self, provider: Provider) -> None:
        if not self._deduplicator.register(provider):
            self._stats.skipped += 1
            return

        if self._current_run_id is None:
            raise RuntimeError("Run ainda nao iniciado.")

        result = self._storage.upsert_provider(
            provider,
            run_id=self._current_run_id,
            profession_id=self._runtime.profession_id,
            region_id=self._runtime.region_id,
        )

        if result.action == "inserted":
            self._stats.inserted += 1
            self._stats.captured += 1
            if self._stats.captured % 10 == 0:
                self._logger.info("Progresso do lote: %s leads capturados.", self._stats.captured)
        elif result.action == "updated":
            self._stats.updated += 1
            self._stats.captured += 1
            if self._stats.captured % 10 == 0:
                self._logger.info("Progresso do lote: %s leads capturados.", self._stats.captured)
        else:
            self._stats.skipped += 1

    def _should_stop(self) -> bool:
        if self._is_stop_requested():
            return True

        effective_limit = self._runtime.max_records or self._settings.max_records_per_run
        return self._stats.captured >= effective_limit

    def _is_stop_requested(self) -> bool:
        return self._stop_event.is_set() or self._check_external_stop()

    def _check_external_stop(self) -> bool:
        if self._external_stop_requested:
            return True

        if self._current_run_id is None:
            return False

        now = time.monotonic()
        if now - self._last_external_stop_check < 2:
            return False

        self._last_external_stop_check = now
        if self._storage.is_stop_requested(self._current_run_id):
            self._external_stop_requested = True
            self._logger.warning("Parada externa solicitada para o lote %s.", self._current_run_id)
            return True

        return False
