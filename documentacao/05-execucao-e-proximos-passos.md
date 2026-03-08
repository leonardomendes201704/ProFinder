# Etapa 5 - Execucao e Proximos Passos

## Ordem recomendada

1. Aplicar `03-bootstrap-mssql.sql` no banco remoto.
2. Instalar dependencias Python:

```powershell
pip install -r crawler/requirements.txt
playwright install
```

3. Definir a connection string ODBC:

```powershell
$env:PROFINDER_SQLSERVER_CONNECTION_STRING='Driver={ODBC Driver 17 for SQL Server};Server=SEU_SQL_HOST,1433;Database=ConsertaPraMimDb;Uid=sa;Pwd={SUA_SENHA};Encrypt=yes;TrustServerCertificate=yes;Connection Timeout=30;'
```

4. Executar o crawler:

```powershell
python crawler/main.py --city "praia grande sp" --service "eletricista" --profession-id 1 --region-id 4
```

## Validacoes feitas nesta etapa

- `py -m compileall crawler` com sucesso
- modelagem .NET adicionada para configuracoes e leads multissite

## Ponto pendente conhecido

- a geracao da migration EF para as novas tabelas ficou bloqueada por um problema de MSBuild/workload no ambiente local ao compilar projetos com referencias encadeadas
- por isso o SQL de bootstrap foi entregue como caminho operacional imediato

## Proximos passos recomendados

- criar a migration EF assim que o ambiente local de build estiver estabilizado
- adicionar API/admin para monitorar `LeadCaptureRun` do crawler multissite
- promover leads consolidados para `prf_professionals`
- enriquecer scrapers com seletores finais por fonte real
- adicionar fila persistente em banco para retomar jobs longos apos falha
