# Etapa 1 - Visao Geral

## Objetivo

Criar um crawler Python multi-site para captar prestadores em:

- Google Maps
- OLX
- Telelistas
- GuiaMais

Persistencia principal:

- SQL Server / MSSQL do ProFinder

## Premissas adotadas

- O dado bruto/consolidado vai para uma tabela generica `prf_provider_leads`.
- Cada execucao abre um lote em `prf_lead_capture_runs`.
- Parametros funcionais do crawler ficam em `prf_app_settings`.
- Os parametros ficam preparados para administracao no painel Razor em `/Settings`.

## Entregas desta fase

- Modelagem .NET para `AppSetting` e `ProviderLead`
- Paginas admin de `Configuracoes` e `Crawler Leads`
- Projeto Python `crawler/`
- Script SQL de bootstrap do schema novo
