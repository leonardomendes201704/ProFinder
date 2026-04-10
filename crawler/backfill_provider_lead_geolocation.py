from __future__ import annotations

import argparse
import os
import sys
from pathlib import Path

from dotenv import load_dotenv

if __package__ in {None, ""}:
    sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from crawler.storage import MssqlStorage
from crawler.utils.logger import configure_logging


def parse_args() -> argparse.Namespace:
    load_dotenv()

    parser = argparse.ArgumentParser(
        description="Recalcula latitude e longitude dos leads a partir dos URLs de origem ja salvos."
    )
    parser.add_argument(
        "--connection-string",
        default=os.getenv("PROFINDER_SQLSERVER_CONNECTION_STRING"),
        help="Connection string ODBC do SQL Server. Tambem pode vir de PROFINDER_SQLSERVER_CONNECTION_STRING.",
    )
    parser.add_argument("--site-key", default="google_maps", help="Fonte alvo para o backfill. Padrao: google_maps.")
    parser.add_argument("--run-id", type=int, help="Filtra por um lote especifico de captura.")
    parser.add_argument("--limit", type=int, help="Limita a quantidade de leads analisados.")
    parser.add_argument("--sample-size", type=int, default=10, help="Quantidade maxima de exemplos exibidos no log.")
    parser.add_argument(
        "--apply",
        action="store_true",
        help="Persiste as coordenadas recalculadas. Sem esta flag, executa apenas simulacao.",
    )
    parser.add_argument("--log-level", default="INFO", help="Nivel de log.")

    args = parser.parse_args()
    if not args.connection_string:
        raise SystemExit("Informe a connection string via --connection-string ou PROFINDER_SQLSERVER_CONNECTION_STRING.")

    return args


def main() -> int:
    args = parse_args()
    logger = configure_logging(args.log_level)

    storage = MssqlStorage(args.connection_string, logger)
    storage.connect()

    try:
        result = storage.backfill_provider_lead_coordinates(
            site_key=args.site_key,
            run_id=args.run_id,
            limit=args.limit,
            apply=args.apply,
            sample_size=max(args.sample_size, 0),
        )
    finally:
        storage.close()

    execution_mode = "aplicado" if args.apply else "simulado"
    logger.info(
        "Backfill %s para site=%s run_id=%s: inspecionados=%s extraiveis=%s alterados=%s atualizados=%s inalterados=%s sem_coordenadas_diretas=%s",
        execution_mode,
        args.site_key,
        args.run_id if args.run_id is not None else "todos",
        result.inspected,
        result.extractable,
        result.changed,
        result.updated,
        result.unchanged,
        result.missing_direct_coordinates,
    )

    for sample in result.samples:
        logger.info(
            "Lead %s lote=%s '%s': (%s, %s) -> (%s, %s)",
            sample.lead_id,
            sample.run_id if sample.run_id is not None else "-",
            sample.name,
            sample.previous_latitude,
            sample.previous_longitude,
            sample.new_latitude,
            sample.new_longitude,
        )

    if not args.apply and result.changed > 0:
        logger.info("Use --apply para persistir as coordenadas recalculadas.")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
