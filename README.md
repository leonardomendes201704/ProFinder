# ProFinder MVP

MVP para captacao e gestao de profissionais por profissao e regiao, com painel administrativo em Razor Pages, API REST em ASP.NET Core e crawler Python multi-site integrado ao banco MSSQL.

## Stack

- .NET 8
- ASP.NET Core Razor Pages
- ASP.NET Core Web API
- SQL Server
- Entity Framework Core
- Bootstrap
- Python 3.11+
- Selenium, Playwright e pyodbc
- Docker
- GitHub Actions

## Estrutura

```text
ProFinder.sln
src/
  ProFinder.Domain/
  ProFinder.Application/
  ProFinder.Infrastructure/
  ProFinder.Api/
  ProFinder.Web/
crawler/
deploy/
documentacao/
```

## Executar sem Docker

Defina a connection string por variavel de ambiente:

```powershell
$env:ConnectionStrings__DefaultConnection='Server=SEU_SQL_HOST,1433;Database=ConsertaPraMimDb;User Id=sa;Password=SUA_SENHA;TrustServerCertificate=True;Encrypt=True;MultipleActiveResultSets=True;Connection Timeout=30;'
```

Restore e build:

```powershell
dotnet restore
dotnet build ProFinder.sln
```

Subir API:

```powershell
dotnet run --project src\ProFinder.Api\ProFinder.Api\ProFinder.Api.csproj
```

Subir Web:

```powershell
dotnet run --project src\ProFinder.Web\ProFinder.Web\ProFinder.Web.csproj
```

Crawler:

```powershell
$env:PROFINDER_SQLSERVER_CONNECTION_STRING='Driver={ODBC Driver 18 for SQL Server};Server=SEU_SQL_HOST,1433;Database=ConsertaPraMimDb;Uid=sa;Pwd={SUA_SENHA};Encrypt=yes;TrustServerCertificate=yes;Connection Timeout=30;'
python crawler/main.py --city "praia grande sp" --service "eletricista"
```

Observacao:

- por padrao, o crawler descarta leads sem telefone ou WhatsApp; a regra pode ser alterada em `Configuracoes` pela chave `crawler.require_phone`

## Docker

Arquivos principais de deploy:

- `docker-compose.yml`
- `src/ProFinder.Web/ProFinder.Web/Dockerfile`
- `src/ProFinder.Api/ProFinder.Api/Dockerfile`
- `.env.example`

Copie `.env.example` para `.env` e preencha os valores:

```env
WEB_PORT=8080
API_PORT=8081
ConnectionStrings__DefaultConnection=Server=SEU_SQL_HOST,1433;Database=ConsertaPraMimDb;User Id=sa;Password=SUA_SENHA;TrustServerCertificate=True;Encrypt=True;MultipleActiveResultSets=True;Connection Timeout=30;
PROFINDER_SQLSERVER_CONNECTION_STRING=Driver={ODBC Driver 18 for SQL Server};Server=SEU_SQL_HOST,1433;Database=ConsertaPraMimDb;Uid=sa;Pwd={SUA_SENHA};Encrypt=yes;TrustServerCertificate=yes;Connection Timeout=30;
ExternalServices__IbgeBaseUrl=https://servicodados.ibge.gov.br
ExternalServices__ViaCepBaseUrl=https://viacep.com.br
ExternalServices__NominatimBaseUrl=https://nominatim.openstreetmap.org
ExternalServices__NominatimUserAgent=ProFinder/1.0 (+https://seu-dominio.com)
RealtimeNotifications__GoogleMapsLeadsWebhookKey=troque-esta-chave
```

Subir containers:

```powershell
docker compose up -d --build
```

Portas padrao:

- painel web: `http://localhost:8080`
- API: `http://localhost:8081`

## Deploy automatico na Hostinger VPS

Arquivos:

- `deploy/bootstrap-vps.sh`
- `deploy/deploy.sh`
- `.github/workflows/deploy.yml`
- `deploy/nginx/profinder.consertapramim.com.conf.example`

Passo inicial na VPS:

```bash
git clone https://github.com/leonardomendes201704/ProFinder.git /opt/profinder
cd /opt/profinder
bash deploy/bootstrap-vps.sh
cp .env.example .env
nano .env
docker compose up -d --build
```

Secrets do GitHub Actions:

- `VPS_HOST`
- `VPS_PORT`
- `VPS_USER`
- `VPS_SSH_KEY`
- `VPS_APP_DIR`

Depois disso, todo push na branch `main` roda:

- `dotnet restore`
- `dotnet build`
- `python -m compileall crawler`
- deploy remoto via SSH
- `docker compose up -d --build`

## Estado atual da VPS

Ambiente configurado:

- VPS: `187.77.48.150`
- diretorio: `/opt/profinder`
- branch de deploy: `main`
- porta interna da Web no host: `127.0.0.1:5200`
- porta interna da API no host: `127.0.0.1:5201`

O Nginx da VPS pode usar o template em `deploy/nginx/profinder.consertapramim.com.conf.example`.

Observacao importante:

- `profinder.consertapramim.com` ainda nao resolve no DNS
- enquanto o DNS nao apontar para a VPS, o HTTPS com Certbot nao pode ser emitido

Quando o DNS estiver apontando para `187.77.48.150`, o passo esperado na VPS e:

```bash
certbot --nginx -d profinder.consertapramim.com
```

## Observacoes de deploy

- o SQL Server permanece externo; o compose nao sobe o banco
- o container do Web inclui Python, Chromium, ChromeDriver e Playwright para suportar o crawler
- o crawler multi-site grava em `prf_provider_leads`
- a tela `Google Maps` usa a tabela `prf_google_maps_leads`, que vem do scraper standalone

## Migrations

Atualizar banco:

```powershell
dotnet ef database update --project src\ProFinder.Infrastructure\ProFinder.Infrastructure\ProFinder.Infrastructure.csproj --startup-project src\ProFinder.Api\ProFinder.Api\ProFinder.Api.csproj
```

Criar migration:

```powershell
dotnet ef migrations add NomeDaMigration --project src\ProFinder.Infrastructure\ProFinder.Infrastructure\ProFinder.Infrastructure.csproj --startup-project src\ProFinder.Api\ProFinder.Api\ProFinder.Api.csproj --output-dir Data\Migrations
```

## Proximos passos sugeridos

- reverse proxy com HTTPS e dominio
- autenticacao de usuarios
- promocao de leads capturados para profissionais
- score de qualificacao
- fila/worker dedicada para crawlers
