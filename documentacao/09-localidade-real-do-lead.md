## Localidade real em ProviderLeads

Em `2026-03-08`, a tela de `ProviderLeads` passou a exibir a localidade real do lead, derivada do endereco capturado.

### O que mudou

- A localidade do lead agora usa `Neighborhood / City / State`
- A extracao acontece de forma centralizada no crawler, no momento do `upsert`
- A coluna `Localidade` da grid nao usa mais a regiao-alvo como valor principal
- A tela de detalhes do lead passou a mostrar `Localidade` em vez de apenas `Cidade/UF`

### Regra de extracao

Padroes como:

- `Rua X, 123 - Bairro, Cidade - UF, 12345-678`
- `Avenida Y - Bairro, Cidade - UF, 12345-678`

sao interpretados para preencher:

- `Neighborhood`
- `City`
- `State`

### Retrocompatibilidade

- Leads novos passam a gravar a localidade extraida automaticamente
- Leads antigos podem ser recalculados por rotina de backfill a partir do endereco ja salvo
