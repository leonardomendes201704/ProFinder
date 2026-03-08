from __future__ import annotations

import asyncio
from dataclasses import dataclass, field
from typing import Any


@dataclass(slots=True)
class CrawlTask:
    site_key: str
    url: str
    mode: str = "http"
    page_number: int = 1
    metadata: dict[str, Any] = field(default_factory=dict)


class AsyncTaskQueue:
    def __init__(self) -> None:
        self._queue: asyncio.Queue[CrawlTask | None] = asyncio.Queue()

    async def put(self, task: CrawlTask | None) -> None:
        await self._queue.put(task)

    async def get(self) -> CrawlTask | None:
        return await self._queue.get()

    def task_done(self) -> None:
        self._queue.task_done()

    async def join(self) -> None:
        await self._queue.join()
