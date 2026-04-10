## Geolocalizacao em ProviderLeads

Em `2026-03-08`, `ProviderLead` passou a persistir:

- `Latitude`
- `Longitude`

### Estrategia

- Google Maps: prioriza as coordenadas reais do lugar (`!3d..!4d..`) e so usa o centro do viewport (`@lat,lng`) como fallback
- Demais fontes: usa geocodificacao por endereco como fallback
- A resolucao roda no momento do `upsert` do lead

### Backfill

- Leads antigos do Google Maps podem ser recalculados a partir dos URLs ja persistidos
- O comando de backfill fica em `crawler/backfill_provider_lead_geolocation.py`
- Exemplo de simulacao por lote:

```powershell
python crawler/backfill_provider_lead_geolocation.py --run-id 17
```

- Exemplo aplicando as atualizacoes:

```powershell
python crawler/backfill_provider_lead_geolocation.py --run-id 17 --apply
```

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
- O backfill atual corrige prioritariamente os leads do `google_maps` sem depender de nova captura
