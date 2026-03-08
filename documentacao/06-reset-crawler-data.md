# Reset de Dados do Crawler

## Objetivo

Adicionar uma limpeza administrativa para zerar os dados operacionais do crawler sem apagar configuracoes do sistema.

## O que o reset apaga

- `prf_provider_leads`
- `prf_google_maps_leads`
- `prf_lead_capture_logs`
- `prf_lead_capture_runs`

## O que o reset preserva

- `prf_app_settings`
- cadastros mestres como profissões, regiões, origens e status
- profissionais já promovidos para o CRM

## Regras de segurança

- o reset exige a confirmacao textual `ZERAR`
- processos Python iniciados pela UI sao encerrados antes da limpeza
- se existir lote ativo em outra execucao/instancia, o reset e bloqueado

## Comportamento da UI

- o botao fica em `/CrawlerRuns/Index`
- depois do reset, a listagem de `Crawler Leads` recebe notificacao via SignalR e atualiza sem refresh manual
- os identificadores das tabelas resetadas sao reseedados para reiniciar a contagem
