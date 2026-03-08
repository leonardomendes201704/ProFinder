# Etapa 04 - UI para disparo do crawler

## Objetivo

Adicionar no Admin uma interface simples para:

- listar os lotes do crawler multi-site
- iniciar um novo lote sem linha de comando
- acompanhar status, contadores e erros do processo

## O que foi implementado

- Pagina de lotes: `/CrawlerRuns/Index`
- Pagina de criacao: `/CrawlerRuns/Create`
- Pagina de detalhes: `/CrawlerRuns/Details/{id}`
- Integracao com o `LeadCaptureRun` existente para historico
- Disparo do `crawler/main.py` em background via `ProcessStartInfo`
- Reaproveitamento do mesmo `run_id` entre Web e Python
- Auto refresh na tela de detalhes enquanto o lote estiver em `Queued`, `Starting` ou `Running`

## Configuracoes persistidas em banco

Os parametros do launcher ficam em `prf_app_settings`, categoria `CrawlerLauncher`:

- `crawler.launcher.python_executable`
- `crawler.launcher.script_path`
- `crawler.launcher.working_directory`
- `crawler.launcher.export_directory`
- `crawler.launcher.sql_driver`
- `crawler.launcher.connection_string_override`
- `crawler.launcher.default_sites_csv`
- `crawler.launcher.log_level`

Esses parametros aparecem na UI em `/Settings/Index`.

## Fluxo de execucao

1. O usuario abre `/CrawlerRuns/Create`
2. Informa cidade, servico, sites, vinculos opcionais e overrides
3. A aplicacao cria um registro `Queued` em `prf_lead_capture_runs`
4. O Web dispara o processo Python em background
5. O Python recebe `--run-id` e atualiza o mesmo lote para `Running`
6. O crawler persiste leads em `prf_provider_leads`
7. Ao final, o Python marca o lote como `Completed` ou `Failed`

## Observacoes tecnicas

- O launcher tenta localizar automaticamente a raiz da solution quando `working_directory` estiver vazio.
- Se `crawler.launcher.connection_string_override` estiver vazio, a aplicacao converte a `DefaultConnection` do .NET para ODBC.
- Se o processo Python nao iniciar, o lote e marcado como `Failed`.

## Proximo passo recomendado

- adicionar SignalR para atualizar a tela de detalhes do lote sem polling
- exibir stdout/stderr resumido do processo em uma tabela propria
- permitir reenfileirar um lote com os mesmos parametros
