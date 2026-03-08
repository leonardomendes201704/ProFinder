# Etapa 05 - Logs persistidos, SignalR e parada do crawler

## Objetivo

Adicionar observabilidade e controle operacional ao launcher do crawler:

- salvar logs do processo Python em banco
- exibir os logs em tempo real na UI via SignalR
- permitir solicitar a parada do lote pelo Admin

## O que foi implementado

- Nova tabela `prf_lead_capture_logs`
- Hub SignalR para lotes do crawler
- Console ao vivo em `/CrawlerRuns/Details/{id}`
- Botao `Parar crawler` quando o lote estiver ativo
- Registro local do processo Python no Web app
- Fallback de parada via banco: o Python consulta `prf_lead_capture_runs.Status = 'Stopping'`

## Fluxo dos logs

1. O Admin inicia um lote em `/CrawlerRuns/Create`
2. O Web inicia o processo Python com `RedirectStandardOutput` e `RedirectStandardError`
3. Cada linha recebida e salva em `prf_lead_capture_logs`
4. A linha e enviada via SignalR para o grupo do lote
5. A tela de detalhes atualiza o console sem refresh

## Fluxo da parada

1. O usuario clica em `Parar crawler`
2. O lote muda para `Stopping`
3. Um log operacional e gravado em banco
4. Se o processo local estiver registrado, o launcher encerra a arvore de processos
5. Se o processo continuar vivo ou estiver em outra instancia, o Python para ao detectar `Stopping` no banco
6. O lote termina como `Stopped`

## Arquivos principais

- `src/ProFinder.Domain/ProFinder.Domain/Entities/LeadCaptureLog.cs`
- `src/ProFinder.Infrastructure/ProFinder.Infrastructure/Services/CrawlerRunService.cs`
- `src/ProFinder.Infrastructure/ProFinder.Infrastructure/Services/CrawlerProcessRegistry.cs`
- `src/ProFinder.Web/ProFinder.Web/Realtime/CrawlerRunSignalRNotifier.cs`
- `src/ProFinder.Web/ProFinder.Web/Hubs/CrawlerRunsHub.cs`
- `src/ProFinder.Web/ProFinder.Web/Pages/CrawlerRuns/Details.cshtml`
- `src/ProFinder.Web/ProFinder.Web/wwwroot/js/crawler-run-live.js`

## Observacoes

- O console em tempo real depende do lote ser iniciado pela UI do Admin.
- Lotes iniciados diretamente por CLI continuam funcionando, mas o streaming via SignalR nao existe nesse fluxo.
- A parada e mais rapida quando o processo esta na mesma instancia do Web app.
