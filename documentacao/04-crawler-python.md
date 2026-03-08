# Etapa 4 - Projeto Python

## Estrutura entregue

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

## Fluxo

1. `main.py` le argumentos e conecta no MSSQL.
2. `storage.py` carrega `prf_app_settings`.
3. `scheduler.py` abre um lote em `prf_lead_capture_runs`.
4. Cada scraper gera tarefas iniciais por cidade + servico.
5. O scheduler processa as tarefas de forma concorrente.
6. Cada provider e consolidado em `prf_provider_leads`.
7. Ao final, o run exporta `providers.csv` e `providers.json`.

## Persistencia durante a execucao

- para `google_maps`, cada lead agora e persistido logo apos a extracao do card
- isso evita esperar o fim da task inteira do Selenium para comecar a gravar no MSSQL
- os demais scrapers continuam retornando lotes em memoria e sao persistidos ao final de cada task

## Estrategia por fonte

### Google Maps

- Selenium
- scroll progressivo
- coleta de nome, telefone, endereco, website, rating e reviews

### OLX

- HTTP first
- fallback opcional para Playwright
- parse de cards/listagens e pagina seguinte

### Telelistas / GuiaMais

- HTTP first
- fallback opcional para Playwright
- parse de cards e paginacao

## Deduplicacao

- chave consolidada: `name + normalized_phone`
- quando telefone nao existe: `name + city`
- o banco guarda a chave em `DeduplicationKey`
- `SourceSitesJson` e `SourceUrlsJson` preservam o historico das fontes consolidadas
