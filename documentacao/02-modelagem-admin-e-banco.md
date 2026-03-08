# Etapa 2 - Modelagem Admin e Banco

## Tabelas novas

### `prf_app_settings`

Uso:

- parametros funcionais do crawler
- valores editaveis pelo admin

Exemplos de chaves:

- `crawler.concurrent_workers`
- `crawler.http_concurrency`
- `crawler.request_delay_ms`
- `crawler.browser_fallback_enabled`
- `crawler.proxy_list_json`
- `crawler.google_maps.max_idle_scrolls`

### `prf_provider_leads`

Uso:

- staging consolidado e deduplicado dos leads multissite

Campos principais:

- `LeadCaptureRunId`
- `LeadSourceId`
- `ProfessionId`
- `RegionId`
- `SiteKey`
- `SearchQuery`
- `DeduplicationKey`
- `Name`
- `Phone`
- `WhatsApp`
- `NormalizedPhone`
- `Address`
- `Neighborhood`
- `City`
- `State`
- `Website`
- `SourceListingUrl`
- `SourceDetailsUrl`
- `SourceSitesJson`
- `SourceUrlsJson`
- `SourceCount`
- `Rating`
- `ReviewCount`
- `RawPayloadJson`

## Paginas admin adicionadas

- `/Settings/Index`
- `/Settings/Edit`
- `/ProviderLeads/Index`
- `/ProviderLeads/Details`

## Observacao importante

O ambiente local de build/EF encontrou um problema de workload/MSBuild ao compilar projetos com referencias encadeadas. Por isso, o bootstrap SQL foi entregue em `03-bootstrap-mssql.sql` para destravar a aplicacao imediata do schema no banco remoto.
