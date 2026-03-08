# 07 - Multiplas profissoes por lead e profissional

## Objetivo

Permitir que o mesmo lead consolidado e o mesmo profissional tenham mais de uma profissao relacionada, sem perder a profissao principal usada hoje pelo sistema.

## Estrategia adotada

- `prf_provider_leads.ProfessionId` continua existindo como profissao principal.
- `prf_professionals.ProfessionId` continua existindo como profissao principal.
- Foram criadas as tabelas de associacao:
  - `prf_provider_lead_professions`
  - `prf_professional_professions`

## Retrocompatibilidade

- Os dashboards, filtros e telas antigas continuam funcionando com `ProfessionId`.
- Os detalhes, listagens e o cadastro de profissionais agora exibem e editam a lista completa de profissoes.
- APIs antigas que enviam apenas `ProfessionId` continuam funcionando: a profissao principal vira tambem a unica profissao relacionada.

## Comportamento do crawler

- Quando um lead deduplicado reaparece em outra busca com outra profissao, o crawler:
  - mantem o mesmo lead consolidado
  - preserva a profissao principal ja existente
  - adiciona a nova profissao em `prf_provider_lead_professions`

Exemplo:

- busca 1: `Joao / telefone X / encanador`
- busca 2: `Joao / telefone X / eletricista`

Resultado:

- um unico registro em `prf_provider_leads`
- duas relacoes em `prf_provider_lead_professions`

## Migracao

A migration `AddMultiProfessionSupport` cria as tabelas e faz backfill:

- `prf_professional_professions` recebe os `ProfessionId` ja existentes de `prf_professionals`
- `prf_provider_lead_professions` recebe os `ProfessionId` ja existentes de `prf_provider_leads`

## Impacto na UI

- cadastro/edicao de profissional:
  - campo de profissao principal
  - multisselecao de profissoes
- listagem/detalhes:
  - exibem a lista agregada de profissoes

## Observacao operacional

Em ambientes existentes, o banco precisa ser atualizado antes do deploy da aplicacao que consome as novas tabelas.
