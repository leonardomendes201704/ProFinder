## Geolocalizacao em ProviderLeads

Em `2026-03-08`, `ProviderLead` passou a persistir:

- `Latitude`
- `Longitude`

### Estrategia

- Google Maps: tenta extrair coordenadas diretamente da URL do detalhe
- Demais fontes: usa geocodificacao por endereco como fallback
- A resolucao roda no momento do `upsert` do lead

### Configuracoes no Admin

As configuracoes ficam em `Configuracoes`:

- `crawler.geolocation_enabled`
- `crawler.geolocation.nominatim_base_url`
- `crawler.geolocation.nominatim_user_agent`
- `crawler.geolocation.timeout_seconds`
- `crawler.geolocation.request_delay_ms`

### Observacoes

- Coordenadas existentes sao preservadas quando nao houver dado novo melhor
- O fallback de geocodificacao usa cache em memoria por execucao do crawler
- A tela de detalhes do lead passou a mostrar as coordenadas e um link para mapa
