# Multi-Site Crawler

Crawler Python 3.11+ para captacao de prestadores em multiplas fontes com persistencia no MSSQL do ProFinder.

## Estrutura

```text
crawler/
  main.py
  config.py
  scheduler.py
  storage.py
  scrapers/
    base.py
    google_maps.py
    olx.py
    telelistas.py
    guiamais.py
  core/
    browser.py
    http_client.py
    queue.py
    deduplicator.py
  models/
    provider.py
  utils/
    parser.py
    phone_extractor.py
    logger.py
```

## Dependencias

```bash
pip install -r crawler/requirements.txt
playwright install
```

## Variavel de ambiente

Use uma connection string ODBC do SQL Server:

```powershell
$env:PROFINDER_SQLSERVER_CONNECTION_STRING='Driver={ODBC Driver 17 for SQL Server};Server=SEU_SQL_HOST,1433;Database=ConsertaPraMimDb;Uid=sa;Pwd={SUA_SENHA};Encrypt=yes;TrustServerCertificate=yes;Connection Timeout=30;'
```

## Execucao

```powershell
python crawler/main.py --city "praia grande sp" --service "eletricista" --profession-id 1 --region-id 4
```

Parametros uteis:

- `--sites google_maps,telelistas`
- `--headless true`
- `--max-records 200`
- `--export-dir crawler/exports`

## Persistencia

O crawler usa as tabelas:

- `prf_app_settings`
- `prf_lead_capture_runs`
- `prf_provider_leads`
- `prf_lead_sources`

## Observacoes

- `Google Maps` usa Selenium.
- `OLX`, `Telelistas` e `GuiaMais` usam HTTP first com fallback opcional para Playwright.
- Os seletores HTML de guias publicos mudam com frequencia. Ajustes finos por fonte continuam esperados.
