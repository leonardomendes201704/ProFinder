## Regra de telefone obrigatorio no crawler

Em `2026-03-08`, o crawler passou a tratar telefone como criterio configuravel de captacao.

### O que mudou

- Nova configuracao persistida: `crawler.require_phone`
- Categoria: `Crawler`
- Valor padrao: `true`
- Edicao disponivel na UI do Admin em `Configuracoes`

### Comportamento

- Quando `crawler.require_phone = true`, leads sem `Phone` e sem `WhatsApp` sao descartados antes do `upsert` em `prf_provider_leads`
- A regra vale para todas as fontes do crawler multi-site
- Leads descartados entram em `skipped` nas estatisticas do lote

### Retrocompatibilidade

- Ambientes novos recebem a configuracao no seed/bootstrap
- Ambientes existentes recebem a configuracao automaticamente pelo catalogo de settings ao subir a aplicacao
