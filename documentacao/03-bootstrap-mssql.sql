IF OBJECT_ID('dbo.prf_app_settings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.prf_app_settings
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_prf_app_settings PRIMARY KEY,
        [Key] NVARCHAR(150) NOT NULL,
        Category NVARCHAR(80) NOT NULL,
        DisplayName NVARCHAR(120) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        DataType NVARCHAR(20) NOT NULL,
        [Value] NVARCHAR(4000) NOT NULL,
        DefaultValue NVARCHAR(4000) NOT NULL,
        IsSensitive BIT NOT NULL CONSTRAINT DF_prf_app_settings_IsSensitive DEFAULT(0),
        IsEditable BIT NOT NULL CONSTRAINT DF_prf_app_settings_IsEditable DEFAULT(1),
        DisplayOrder INT NOT NULL CONSTRAINT DF_prf_app_settings_DisplayOrder DEFAULT(0),
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NOT NULL
    );

    CREATE UNIQUE INDEX IX_prf_app_settings_Key ON dbo.prf_app_settings([Key]);
    CREATE INDEX IX_prf_app_settings_Category_DisplayOrder ON dbo.prf_app_settings(Category, DisplayOrder);
END;
GO

IF OBJECT_ID('dbo.prf_provider_leads', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.prf_provider_leads
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_prf_provider_leads PRIMARY KEY,
        LeadCaptureRunId INT NOT NULL,
        LeadSourceId INT NOT NULL,
        ProfessionId INT NULL,
        RegionId INT NULL,
        ImportedProfessionalId INT NULL,
        SiteKey NVARCHAR(40) NOT NULL,
        SearchQuery NVARCHAR(250) NOT NULL,
        DeduplicationKey NVARCHAR(200) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Phone NVARCHAR(30) NULL,
        WhatsApp NVARCHAR(30) NULL,
        NormalizedPhone NVARCHAR(20) NULL,
        [Address] NVARCHAR(300) NULL,
        Neighborhood NVARCHAR(120) NULL,
        City NVARCHAR(120) NULL,
        [State] NVARCHAR(10) NULL,
        Website NVARCHAR(250) NULL,
        SourceListingUrl NVARCHAR(500) NULL,
        SourceDetailsUrl NVARCHAR(500) NULL,
        ExternalId NVARCHAR(120) NULL,
        ImportStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_prf_provider_leads_ImportStatus DEFAULT('Captured'),
        SourceSitesJson NVARCHAR(2000) NULL,
        SourceUrlsJson NVARCHAR(4000) NULL,
        SourceCount INT NOT NULL CONSTRAINT DF_prf_provider_leads_SourceCount DEFAULT(1),
        Rating DECIMAL(5,2) NULL,
        ReviewCount INT NULL,
        RawPayloadJson NVARCHAR(MAX) NULL,
        ScrapedAt DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_prf_provider_leads_capture_runs FOREIGN KEY (LeadCaptureRunId) REFERENCES dbo.prf_lead_capture_runs(Id),
        CONSTRAINT FK_prf_provider_leads_sources FOREIGN KEY (LeadSourceId) REFERENCES dbo.prf_lead_sources(Id),
        CONSTRAINT FK_prf_provider_leads_professions FOREIGN KEY (ProfessionId) REFERENCES dbo.prf_professions(Id),
        CONSTRAINT FK_prf_provider_leads_regions FOREIGN KEY (RegionId) REFERENCES dbo.prf_regions(Id),
        CONSTRAINT FK_prf_provider_leads_professionals FOREIGN KEY (ImportedProfessionalId) REFERENCES dbo.prf_professionals(Id)
    );

    CREATE UNIQUE INDEX IX_prf_provider_leads_DeduplicationKey ON dbo.prf_provider_leads(DeduplicationKey);
    CREATE INDEX IX_prf_provider_leads_NormalizedPhone ON dbo.prf_provider_leads(NormalizedPhone);
    CREATE INDEX IX_prf_provider_leads_SiteKey ON dbo.prf_provider_leads(SiteKey);
    CREATE INDEX IX_prf_provider_leads_City ON dbo.prf_provider_leads(City);
    CREATE INDEX IX_prf_provider_leads_ImportStatus ON dbo.prf_provider_leads(ImportStatus);
    CREATE INDEX IX_prf_provider_leads_LeadCaptureRunId ON dbo.prf_provider_leads(LeadCaptureRunId);
    CREATE INDEX IX_prf_provider_leads_LeadSourceId ON dbo.prf_provider_leads(LeadSourceId);
END;
GO

IF OBJECT_ID('dbo.prf_professional_professions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.prf_professional_professions
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_prf_professional_professions PRIMARY KEY,
        ProfessionalId INT NOT NULL,
        ProfessionId INT NOT NULL,
        IsPrimary BIT NOT NULL CONSTRAINT DF_prf_professional_professions_IsPrimary DEFAULT(0),
        CONSTRAINT FK_prf_professional_professions_professionals FOREIGN KEY (ProfessionalId) REFERENCES dbo.prf_professionals(Id) ON DELETE CASCADE,
        CONSTRAINT FK_prf_professional_professions_professions FOREIGN KEY (ProfessionId) REFERENCES dbo.prf_professions(Id)
    );

    CREATE UNIQUE INDEX IX_prf_professional_professions_ProfessionalId_ProfessionId
        ON dbo.prf_professional_professions(ProfessionalId, ProfessionId);
    CREATE INDEX IX_prf_professional_professions_ProfessionId
        ON dbo.prf_professional_professions(ProfessionId);
END;
GO

IF OBJECT_ID('dbo.prf_provider_lead_professions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.prf_provider_lead_professions
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_prf_provider_lead_professions PRIMARY KEY,
        ProviderLeadId INT NOT NULL,
        ProfessionId INT NOT NULL,
        IsPrimary BIT NOT NULL CONSTRAINT DF_prf_provider_lead_professions_IsPrimary DEFAULT(0),
        CONSTRAINT FK_prf_provider_lead_professions_provider_leads FOREIGN KEY (ProviderLeadId) REFERENCES dbo.prf_provider_leads(Id) ON DELETE CASCADE,
        CONSTRAINT FK_prf_provider_lead_professions_professions FOREIGN KEY (ProfessionId) REFERENCES dbo.prf_professions(Id)
    );

    CREATE UNIQUE INDEX IX_prf_provider_lead_professions_ProviderLeadId_ProfessionId
        ON dbo.prf_provider_lead_professions(ProviderLeadId, ProfessionId);
    CREATE INDEX IX_prf_provider_lead_professions_ProfessionId
        ON dbo.prf_provider_lead_professions(ProfessionId);
END;
GO

INSERT INTO dbo.prf_professional_professions
(
    ProfessionalId,
    ProfessionId,
    IsPrimary
)
SELECT p.Id,
       p.ProfessionId,
       1
  FROM dbo.prf_professionals p
 WHERE NOT EXISTS
(
    SELECT 1
      FROM dbo.prf_professional_professions pp
     WHERE pp.ProfessionalId = p.Id
       AND pp.ProfessionId = p.ProfessionId
);
GO

INSERT INTO dbo.prf_provider_lead_professions
(
    ProviderLeadId,
    ProfessionId,
    IsPrimary
)
SELECT pl.Id,
       pl.ProfessionId,
       1
  FROM dbo.prf_provider_leads pl
 WHERE pl.ProfessionId IS NOT NULL
   AND NOT EXISTS
(
    SELECT 1
      FROM dbo.prf_provider_lead_professions plp
     WHERE plp.ProviderLeadId = pl.Id
       AND plp.ProfessionId = pl.ProfessionId
);
GO

IF OBJECT_ID('dbo.prf_lead_capture_logs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.prf_lead_capture_logs
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_prf_lead_capture_logs PRIMARY KEY,
        LeadCaptureRunId INT NOT NULL,
        LogLevel NVARCHAR(20) NOT NULL,
        [Source] NVARCHAR(30) NOT NULL,
        [Message] NVARCHAR(4000) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_prf_lead_capture_logs_capture_runs FOREIGN KEY (LeadCaptureRunId) REFERENCES dbo.prf_lead_capture_runs(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_prf_lead_capture_logs_LeadCaptureRunId ON dbo.prf_lead_capture_logs(LeadCaptureRunId);
    CREATE INDEX IX_prf_lead_capture_logs_CreatedAt ON dbo.prf_lead_capture_logs(CreatedAt);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.prf_lead_sources WHERE Name = 'OLX')
BEGIN
    INSERT INTO dbo.prf_lead_sources (Name, Description, Url, IsActive)
    VALUES ('OLX', 'Lead captado em anuncios publicos da OLX.', 'https://www.olx.com.br', 1);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.prf_lead_sources WHERE Name = 'Telelistas')
BEGIN
    INSERT INTO dbo.prf_lead_sources (Name, Description, Url, IsActive)
    VALUES ('Telelistas', 'Lead captado no guia Telelistas.', 'https://www.telelistas.net', 1);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.prf_lead_sources WHERE Name = 'GuiaMais')
BEGIN
    INSERT INTO dbo.prf_lead_sources (Name, Description, Url, IsActive)
    VALUES ('GuiaMais', 'Lead captado no guia GuiaMais.', 'https://www.guiamais.com.br', 1);
END;
GO

DECLARE @createdAt DATETIME2 = SYSUTCDATETIME();

IF NOT EXISTS (SELECT 1 FROM dbo.prf_app_settings WHERE [Key] = 'crawler.concurrent_workers')
BEGIN
    INSERT INTO dbo.prf_app_settings ([Key], Category, DisplayName, [Description], DataType, [Value], DefaultValue, IsSensitive, IsEditable, DisplayOrder, CreatedAt, UpdatedAt)
    VALUES
    ('crawler.concurrent_workers', 'Crawler', 'Workers concorrentes', 'Quantidade maxima de workers assincronos para o scheduler.', 'int', '4', '4', 0, 1, 1, @createdAt, @createdAt),
    ('crawler.http_concurrency', 'Crawler', 'Concorrencia HTTP', 'Limite de requisicoes HTTP simultaneas.', 'int', '12', '12', 0, 1, 2, @createdAt, @createdAt),
    ('crawler.http_timeout_seconds', 'Crawler', 'Timeout HTTP (segundos)', 'Timeout padrao das requisicoes HTTP.', 'int', '30', '30', 0, 1, 3, @createdAt, @createdAt),
    ('crawler.max_retries', 'Crawler', 'Maximo de tentativas', 'Numero maximo de retries por requisicao.', 'int', '3', '3', 0, 1, 4, @createdAt, @createdAt),
    ('crawler.request_delay_ms', 'Crawler', 'Delay entre requisicoes (ms)', 'Intervalo minimo entre chamadas para a mesma fonte.', 'int', '700', '700', 0, 1, 5, @createdAt, @createdAt),
    ('crawler.browser_fallback_enabled', 'Crawler', 'Fallback de navegador', 'Permite trocar de HTTP para browser automation quando a fonte for dinamica.', 'bool', 'true', 'true', 0, 1, 6, @createdAt, @createdAt),
    ('crawler.user_agent_rotation_enabled', 'Crawler', 'Rotacao de user-agent', 'Habilita rotacao simples de user-agents.', 'bool', 'true', 'true', 0, 1, 7, @createdAt, @createdAt),
    ('crawler.proxy_list_json', 'Crawler', 'Lista de proxies', 'JSON com proxies opcionais para rotacao.', 'json', '[]', '[]', 0, 1, 8, @createdAt, @createdAt),
    ('crawler.max_pages_per_target', 'Crawler', 'Maximo de paginas por alvo', 'Limite de paginas a percorrer por site e alvo.', 'int', '50', '50', 0, 1, 9, @createdAt, @createdAt),
    ('crawler.max_records_per_run', 'Crawler', 'Maximo de leads por execucao', 'Quantidade maxima de registros persistidos por execucao.', 'int', '5000', '5000', 0, 1, 10, @createdAt, @createdAt),
    ('crawler.browser_headless', 'Crawler', 'Executar navegadores em headless', 'Controla se Selenium e Playwright rodam em modo headless.', 'bool', 'true', 'true', 0, 1, 11, @createdAt, @createdAt),
    ('crawler.google_maps.max_idle_scrolls', 'GoogleMaps', 'Scrolls ociosos maximos', 'Limite de scrolls sem novos cards no Google Maps.', 'int', '8', '8', 0, 1, 1, @createdAt, @createdAt),
    ('crawler.google_maps.scroll_pause_ms', 'GoogleMaps', 'Pausa do scroll (ms)', 'Pausa entre scrolls da lista do Google Maps.', 'int', '1500', '1500', 0, 1, 2, @createdAt, @createdAt);
END;
GO

DECLARE @createdAt DATETIME2 = SYSUTCDATETIME();

IF NOT EXISTS (SELECT 1 FROM dbo.prf_app_settings WHERE [Key] = 'crawler.launcher.python_executable')
BEGIN
    INSERT INTO dbo.prf_app_settings ([Key], Category, DisplayName, [Description], DataType, [Value], DefaultValue, IsSensitive, IsEditable, DisplayOrder, CreatedAt, UpdatedAt)
    VALUES
    ('crawler.launcher.python_executable', 'CrawlerLauncher', 'Executavel do Python', 'Comando ou caminho do Python usado para iniciar o crawler pela UI.', 'string', 'python', 'python', 0, 1, 1, @createdAt, @createdAt),
    ('crawler.launcher.script_path', 'CrawlerLauncher', 'Caminho do script do crawler', 'Caminho absoluto ou relativo do arquivo crawler/main.py.', 'string', 'crawler/main.py', 'crawler/main.py', 0, 1, 2, @createdAt, @createdAt),
    ('crawler.launcher.working_directory', 'CrawlerLauncher', 'Diretorio de trabalho do crawler', 'Diretorio base para iniciar o processo. Em branco usa a raiz da solution quando encontrada.', 'string', '', '', 0, 1, 3, @createdAt, @createdAt),
    ('crawler.launcher.export_directory', 'CrawlerLauncher', 'Diretorio de exportacao', 'Diretorio onde o crawler grava providers.csv e providers.json.', 'string', 'crawler/exports', 'crawler/exports', 0, 1, 4, @createdAt, @createdAt),
    ('crawler.launcher.sql_driver', 'CrawlerLauncher', 'Driver ODBC do SQL Server', 'Driver usado para converter a connection string da aplicacao em connection string ODBC para o Python.', 'string', 'ODBC Driver 17 for SQL Server', 'ODBC Driver 17 for SQL Server', 0, 1, 5, @createdAt, @createdAt),
    ('crawler.launcher.connection_string_override', 'CrawlerLauncher', 'Connection string ODBC override', 'Quando preenchida, substitui a conversao automatica da connection string do .NET ao iniciar o crawler.', 'string', '', '', 1, 1, 6, @createdAt, @createdAt),
    ('crawler.launcher.default_sites_csv', 'CrawlerLauncher', 'Sites padrao do launcher', 'Lista CSV dos sites marcados por padrao no formulario do crawler.', 'string', 'google_maps,olx,telelistas,guiamais', 'google_maps,olx,telelistas,guiamais', 0, 1, 7, @createdAt, @createdAt),
    ('crawler.launcher.log_level', 'CrawlerLauncher', 'Nivel de log do launcher', 'Nivel de log passado para o crawler Python ao iniciar lotes pela UI.', 'string', 'INFO', 'INFO', 0, 1, 8, @createdAt, @createdAt);
END;
GO
