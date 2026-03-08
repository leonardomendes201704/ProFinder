import argparse
import csv
import json
import os
import re
import ssl
import sys
import time
from datetime import datetime, timezone
from pathlib import Path
from urllib import error, request
from urllib.parse import quote

try:
    import pyodbc
except ImportError as exc:  # pragma: no cover - runtime dependency
    raise SystemExit("Dependência ausente: instale pyodbc para gravar no SQL Server.") from exc

from selenium import webdriver
from selenium.common.exceptions import TimeoutException
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.common.by import By
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.support.ui import WebDriverWait
from webdriver_manager.chrome import ChromeDriverManager


DEFAULT_LEAD_SOURCE_ID = 3
DEFAULT_CAPTURE_TYPE = "GoogleMapsScrape"
DEFAULT_CREATED_BY = "GoogleMapsScrapper"
DEFAULT_IMPORT_STATUS = "Captured"


def parse_args():
    parser = argparse.ArgumentParser(
        description="Coleta leads do Google Maps e grava em prf_google_maps_leads."
    )
    parser.add_argument("--query", required=True, help="Consulta usada no Google Maps.")
    parser.add_argument("--profession-id", type=int, help="Profissão alvo do lote.")
    parser.add_argument("--region-id", type=int, help="Região alvo do lote.")
    parser.add_argument("--lead-source-id", type=int, default=DEFAULT_LEAD_SOURCE_ID, help="Origem do lead no banco.")
    parser.add_argument("--created-by", default=DEFAULT_CREATED_BY, help="Identificador do executor.")
    parser.add_argument("--export-csv", help="Caminho opcional para exportar os dados em CSV.")
    parser.add_argument("--headless", action="store_true", help="Executa o Chrome em modo headless.")
    parser.add_argument("--max-idle-scrolls", type=int, default=3, help="Número de scrolls sem novos cards antes de encerrar.")
    parser.add_argument(
        "--notify-url",
        default=os.getenv("GOOGLE_MAPS_NOTIFY_URL"),
        help="Webhook opcional do ProFinder Web para notificação realtime.",
    )
    parser.add_argument(
        "--notify-key",
        default=os.getenv("GOOGLE_MAPS_NOTIFY_KEY"),
        help="Chave opcional enviada no header X-Webhook-Key.",
    )
    parser.add_argument(
        "--notify-batch-size",
        type=int,
        default=5,
        help="Quantidade de mudanças antes de disparar uma notificação realtime.",
    )
    parser.add_argument(
        "--notify-insecure",
        action="store_true",
        help="Desabilita a validação SSL do webhook realtime. Use apenas em desenvolvimento local.",
    )
    parser.add_argument(
        "--connection-string",
        default=os.getenv("GOOGLE_MAPS_SQLSERVER_CONNECTION_STRING"),
        help="Connection string ODBC para SQL Server. Também pode vir da env GOOGLE_MAPS_SQLSERVER_CONNECTION_STRING.",
    )
    return parser.parse_args()


def utcnow():
    return datetime.now(timezone.utc).replace(tzinfo=None)


def iniciar_driver(headless=False):
    options = Options()
    options.add_argument("--start-maximized")
    options.add_argument("--disable-blink-features=AutomationControlled")
    options.add_argument("--lang=pt-BR")
    browser_binary = os.getenv("CHROME_BIN") or os.getenv("GOOGLE_CHROME_BIN")
    driver_path = os.getenv("CHROMEDRIVER_PATH")

    if headless:
        options.add_argument("--headless=new")
        options.add_argument("--window-size=1920,1080")

    if browser_binary:
        options.binary_location = browser_binary

    driver = webdriver.Chrome(
        service=Service(driver_path) if driver_path else Service(ChromeDriverManager().install()),
        options=options,
    )

    return driver


def abrir_conexao(connection_string):
    if not connection_string:
        raise ValueError(
            "Informe a connection string ODBC via --connection-string ou GOOGLE_MAPS_SQLSERVER_CONNECTION_STRING."
        )

    return pyodbc.connect(connection_string, autocommit=False)


def criar_lote(conn, args):
    source_url = f"https://www.google.com/maps/search/{quote(args.query)}"
    now = utcnow()
    cursor = conn.cursor()
    cursor.execute(
        """
        INSERT INTO prf_lead_capture_runs
        (
            LeadSourceId,
            CaptureType,
            Status,
            SourceUrl,
            SearchQuery,
            Notes,
            CreatedBy,
            StartedAt,
            CompletedAt,
            CreatedAt,
            UpdatedAt,
            ItemsCaptured,
            ItemsInserted,
            ItemsUpdated,
            ItemsSkipped
        )
        OUTPUT INSERTED.Id
        VALUES (?, ?, ?, ?, ?, ?, ?, ?, NULL, ?, ?, 0, 0, 0, 0)
        """,
        args.lead_source_id,
        DEFAULT_CAPTURE_TYPE,
        "Running",
        source_url,
        args.query.strip(),
        "Captura automatizada via Selenium + Google Maps.",
        args.created_by.strip(),
        now,
        now,
        now,
    )
    row = cursor.fetchone()
    conn.commit()
    return int(row[0]), source_url


def finalizar_lote(conn, run_id, stats, status="Completed", error_message=None):
    now = utcnow()
    conn.cursor().execute(
        """
        UPDATE prf_lead_capture_runs
           SET Status = ?,
               CompletedAt = ?,
               UpdatedAt = ?,
               ErrorMessage = ?,
               ItemsCaptured = ?,
               ItemsInserted = ?,
               ItemsUpdated = ?,
               ItemsSkipped = ?
         WHERE Id = ?
        """,
        status,
        now,
        now,
        truncate(error_message, 2000),
        stats["captured"],
        stats["inserted"],
        stats["updated"],
        stats["skipped"],
        run_id,
    )
    conn.commit()


def notificar_webhook(args, run_id, stats, status="Running", completed=False, message=None):
    if not args.notify_url:
        return

    payload = {
        "runId": run_id,
        "searchQuery": args.query,
        "status": status,
        "completed": completed,
        "captured": stats["captured"],
        "inserted": stats["inserted"],
        "updated": stats["updated"],
        "skipped": stats["skipped"],
        "message": message,
        "occurredAtUtc": datetime.now(timezone.utc).isoformat(),
    }

    body = json.dumps(payload, ensure_ascii=False).encode("utf-8")
    headers = {
        "Content-Type": "application/json",
    }

    if args.notify_key:
        headers["X-Webhook-Key"] = args.notify_key

    req = request.Request(args.notify_url, data=body, headers=headers, method="POST")

    try:
        ssl_context = ssl._create_unverified_context() if args.notify_insecure else None
        with request.urlopen(req, timeout=10, context=ssl_context) as response:
            if response.status >= 400:
                print(f"Webhook retornou status {response.status}.", file=sys.stderr)
    except error.URLError as exc:
        print(f"Falha ao notificar webhook realtime: {exc}", file=sys.stderr)


def scroll_lista(driver):
    scroll_box = WebDriverWait(driver, 20).until(
        EC.presence_of_element_located((By.XPATH, '//div[@role="feed"]'))
    )

    driver.execute_script(
        "arguments[0].scrollTop = arguments[0].scrollHeight",
        scroll_box,
    )

    time.sleep(2)


def extrair_info(driver, place_url, query):
    data = {
        "place_url": place_url,
        "search_query": query,
        "scraped_at": utcnow().isoformat(),
    }

    try:
        data["name"] = driver.find_element(By.CLASS_NAME, "DUwDvf").text.strip()
    except Exception:
        data["name"] = None

    try:
        data["rating"] = driver.find_element(By.CLASS_NAME, "MW4etd").text.strip()
    except Exception:
        data["rating"] = None

    try:
        data["reviews"] = driver.find_element(By.CLASS_NAME, "UY7F9").text.strip()
    except Exception:
        data["reviews"] = None

    try:
        data["phone"] = driver.find_element(
            By.XPATH, '//button[contains(@data-item-id,"phone")]'
        ).text.strip()
    except Exception:
        data["phone"] = None

    try:
        data["address"] = driver.find_element(
            By.XPATH, '//button[contains(@data-item-id,"address")]'
        ).text.strip()
    except Exception:
        data["address"] = None

    try:
        data["website"] = driver.find_element(
            By.XPATH, '//a[contains(@data-item-id,"authority")]'
        ).get_attribute("href")
    except Exception:
        data["website"] = None

    return data


def coletar(driver, args, conn, run_id):
    resultados = []
    visitados = set()
    idle_scrolls = 0
    stats = {"captured": 0, "inserted": 0, "updated": 0, "skipped": 0}

    while idle_scrolls < args.max_idle_scrolls:
        cards = driver.find_elements(By.XPATH, '//a[contains(@href,"/place/")]')
        novos = False

        for card in cards:
            link = card.get_attribute("href")
            if not link or link in visitados:
                continue

            visitados.add(link)
            novos = True

            driver.execute_script("arguments[0].scrollIntoView();", card)
            time.sleep(1)

            try:
                card.click()
            except Exception:
                driver.execute_script("arguments[0].click();", card)

            try:
                WebDriverWait(driver, 10).until(
                    EC.presence_of_element_located((By.CLASS_NAME, "DUwDvf"))
                )
            except TimeoutException:
                stats["skipped"] += 1
                print("Ignorado: detalhe não carregou para", link)
                continue

            time.sleep(2)
            info = extrair_info(driver, link, args.query)
            acao = gravar_lead(conn, run_id, args, info)

            if acao == "inserted":
                stats["inserted"] += 1
                stats["captured"] += 1
            elif acao == "updated":
                stats["updated"] += 1
                stats["captured"] += 1
            else:
                stats["skipped"] += 1

            resultados.append(info)
            print(f'{len(resultados)} -> {info.get("name") or "Sem nome"} [{acao}]')

            if acao in {"inserted", "updated"} and args.notify_batch_size > 0:
                total_changes = stats["inserted"] + stats["updated"]
                if total_changes % args.notify_batch_size == 0:
                    notificar_webhook(
                        args,
                        run_id,
                        stats,
                        status="Running",
                        completed=False,
                        message=f"Lote parcial com {total_changes} alterações persistidas.",
                    )

        if novos:
            idle_scrolls = 0
        else:
            idle_scrolls += 1
            print("Scrollando para carregar mais...")
            scroll_lista(driver)
            time.sleep(3)

    return resultados, stats


def gravar_lead(conn, run_id, args, info):
    name = truncate(info.get("name"), 200)
    place_url = truncate(info.get("place_url"), 500)

    if not name or not place_url:
        conn.rollback()
        return "skipped"

    normalized_phone = normalize_phone(info.get("phone"))
    now = utcnow()
    payload_json = json.dumps(info, ensure_ascii=False)
    rating = normalize_rating(info.get("rating"))
    review_count = normalize_review_count(info.get("reviews"))

    cursor = conn.cursor()

    existing_id = buscar_lead_existente(cursor, place_url, normalized_phone, name)

    if existing_id:
        cursor.execute(
            """
            UPDATE prf_google_maps_leads
               SET LeadCaptureRunId = ?,
                   LeadSourceId = ?,
                   ProfessionId = ?,
                   RegionId = ?,
                   SearchQuery = ?,
                   Name = ?,
                   Phone = ?,
                   NormalizedPhone = ?,
                   Address = ?,
                   Website = ?,
                   Rating = ?,
                   ReviewCount = ?,
                   RawPayloadJson = ?,
                   ScrapedAt = ?,
                   UpdatedAt = ?,
                   ImportStatus = CASE
                       WHEN ImportStatus IN ('Imported', 'Ignored') THEN ImportStatus
                       ELSE ?
                   END
             WHERE Id = ?
            """,
            run_id,
            args.lead_source_id,
            args.profession_id,
            args.region_id,
            truncate(args.query, 200),
            name,
            truncate(info.get("phone"), 30),
            normalized_phone,
            truncate(info.get("address"), 300),
            truncate(info.get("website"), 300),
            rating,
            review_count,
            payload_json,
            now,
            now,
            DEFAULT_IMPORT_STATUS,
            existing_id,
        )
        conn.commit()
        return "updated"

    cursor.execute(
        """
        INSERT INTO prf_google_maps_leads
        (
            LeadCaptureRunId,
            LeadSourceId,
            ProfessionId,
            RegionId,
            SearchQuery,
            PlaceUrl,
            Name,
            Phone,
            NormalizedPhone,
            Address,
            Website,
            Rating,
            ReviewCount,
            ImportStatus,
            RawPayloadJson,
            ScrapedAt,
            CreatedAt,
            UpdatedAt
        )
        VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        """,
        run_id,
        args.lead_source_id,
        args.profession_id,
        args.region_id,
        truncate(args.query, 200),
        place_url,
        name,
        truncate(info.get("phone"), 30),
        normalized_phone,
        truncate(info.get("address"), 300),
        truncate(info.get("website"), 300),
        rating,
        review_count,
        DEFAULT_IMPORT_STATUS,
        payload_json,
        now,
        now,
        now,
    )
    conn.commit()
    return "inserted"


def buscar_lead_existente(cursor, place_url, normalized_phone, name):
    row = cursor.execute(
        "SELECT TOP 1 Id FROM prf_google_maps_leads WHERE PlaceUrl = ?",
        place_url,
    ).fetchone()

    if row:
        return int(row[0])

    if normalized_phone and name:
        row = cursor.execute(
            """
            SELECT TOP 1 Id
              FROM prf_google_maps_leads
             WHERE NormalizedPhone = ?
               AND Name = ?
            """,
            normalized_phone,
            name,
        ).fetchone()

    return int(row[0]) if row else None


def exportar_csv(dados, arquivo):
    if not arquivo:
        return

    path = Path(arquivo)
    path.parent.mkdir(parents=True, exist_ok=True)

    fieldnames = [
        "name",
        "rating",
        "reviews",
        "phone",
        "address",
        "website",
        "place_url",
        "search_query",
        "scraped_at",
    ]

    with path.open("w", encoding="utf-8-sig", newline="") as csv_file:
        writer = csv.DictWriter(csv_file, fieldnames=fieldnames)
        writer.writeheader()
        for item in dados:
            writer.writerow({field: item.get(field) for field in fieldnames})

    print(f"Arquivo salvo: {path}")


def normalize_phone(value):
    if not value:
        return None

    digits = re.sub(r"\D", "", value)
    return digits or None


def normalize_rating(value):
    if not value:
        return None

    normalized = value.replace(",", ".")
    match = re.search(r"\d+(?:\.\d+)?", normalized)
    return float(match.group(0)) if match else None


def normalize_review_count(value):
    if not value:
        return None

    digits = re.findall(r"\d+", value)
    return int("".join(digits)) if digits else None


def truncate(value, max_length):
    if value is None:
        return None

    text = str(value).strip()
    return text[:max_length] if text else None


def main():
    args = parse_args()
    driver = None
    conn = None
    run_id = None
    stats = {"captured": 0, "inserted": 0, "updated": 0, "skipped": 0}

    try:
        conn = abrir_conexao(args.connection_string)
        run_id, source_url = criar_lote(conn, args)

        driver = iniciar_driver(headless=args.headless)
        driver.get(source_url)
        time.sleep(5)

        dados, stats = coletar(driver, args, conn, run_id)
        exportar_csv(dados, args.export_csv)

        finalizar_lote(conn, run_id, stats)
        notificar_webhook(
            args,
            run_id,
            stats,
            status="Completed",
            completed=True,
            message="Captura concluída com sucesso.",
        )
        print("Lote concluído com sucesso.")
        print(json.dumps(stats, ensure_ascii=False, indent=2))
    except Exception as exc:
        if conn and run_id:
            finalizar_lote(conn, run_id, stats, status="Failed", error_message=str(exc))
            notificar_webhook(
                args,
                run_id,
                stats,
                status="Failed",
                completed=True,
                message=str(exc),
            )
        raise
    finally:
        if driver:
            driver.quit()
        if conn:
            conn.close()


if __name__ == "__main__":
    try:
        main()
    except Exception as error:
        print(f"Erro na execução: {error}", file=sys.stderr)
        sys.exit(1)
