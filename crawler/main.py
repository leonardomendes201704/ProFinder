from __future__ import annotations

import asyncio
import sys
from pathlib import Path

if __package__ in {None, ""}:
    sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from crawler.config import CrawlerSettings, parse_args
from crawler.scheduler import CrawlScheduler
from crawler.storage import MssqlStorage
from crawler.utils.logger import configure_logging


async def async_main() -> int:
    runtime = parse_args()
    logger = configure_logging(runtime.log_level)

    storage = MssqlStorage(runtime.connection_string, logger)
    storage.connect()

    try:
        settings = CrawlerSettings.from_database(storage.load_settings())
        if runtime.headless is not None:
            settings.browser_headless = runtime.headless

        scheduler = CrawlScheduler(runtime=runtime, settings=settings, storage=storage, logger=logger)
        run_id, stats = await scheduler.run()
        csv_path, json_path = scheduler.export(run_id)

        if stats.failed > 0:
            logger.warning("Execucao %s finalizada com falhas.", run_id)
        else:
            logger.info("Execucao %s concluida.", run_id)
        logger.info("CSV exportado em %s", csv_path)
        logger.info("JSON exportado em %s", json_path)
        logger.info("Estatisticas finais: %s", stats.as_dict())
        return 1 if stats.failed > 0 else 0
    finally:
        storage.close()


def main() -> int:
    try:
        return asyncio.run(async_main())
    except KeyboardInterrupt:
        print("Execucao interrompida pelo usuario.", file=sys.stderr)
        return 130
    except Exception as exc:
        print(f"Erro no crawler: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
