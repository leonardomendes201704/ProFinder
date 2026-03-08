using Microsoft.EntityFrameworkCore;
using ProFinder.Domain.Entities;
using ProFinder.Domain.Enums;

namespace ProFinder.Infrastructure.Data.Seed;

public static class SeedData
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Profession>().HasData(GetProfessions());
        modelBuilder.Entity<LeadStatus>().HasData(GetStatuses());
        modelBuilder.Entity<LeadSource>().HasData(GetSources());
        modelBuilder.Entity<Region>().HasData(GetRegions());
        modelBuilder.Entity<Professional>().HasData(GetProfessionals());
        modelBuilder.Entity<ProfessionalRegion>().HasData(GetProfessionalRegions());
        modelBuilder.Entity<Interaction>().HasData(GetInteractions());
        modelBuilder.Entity<Evidence>().HasData(GetEvidences());
        modelBuilder.Entity<LeadCaptureRun>().HasData(GetLeadCaptureRuns());
        modelBuilder.Entity<AppSetting>().HasData(GetAppSettings());
    }

    private static Profession[] GetProfessions() =>
    [
        new Profession { Id = 1, Name = "Eletricista", Description = "Profissionais de instalacoes e manutencao eletrica.", IsActive = true },
        new Profession { Id = 2, Name = "Encanador", Description = "Profissionais de hidraulica e reparos em geral.", IsActive = true },
        new Profession { Id = 3, Name = "Pintor", Description = "Profissionais de pintura residencial e comercial.", IsActive = true },
        new Profession { Id = 4, Name = "Chaveiro", Description = "Profissionais especializados em abertura e copia de chaves.", IsActive = true }
    ];

    private static LeadStatus[] GetStatuses() =>
    [
        new LeadStatus { Id = 1, Name = "Novo", Description = "Lead recem-cadastrado.", DisplayOrder = 1, IsActive = true },
        new LeadStatus { Id = 2, Name = "Em analise", Description = "Lead em validacao.", DisplayOrder = 2, IsActive = true },
        new LeadStatus { Id = 3, Name = "Qualificado", Description = "Lead com criterios minimos atendidos.", DisplayOrder = 3, IsActive = true },
        new LeadStatus { Id = 4, Name = "Contatado", Description = "Primeiro contato realizado.", DisplayOrder = 4, IsActive = true },
        new LeadStatus { Id = 5, Name = "Interessado", Description = "Lead demonstrou interesse.", DisplayOrder = 5, IsActive = true },
        new LeadStatus { Id = 6, Name = "Nao interessado", Description = "Lead sem interesse no momento.", DisplayOrder = 6, IsActive = true },
        new LeadStatus { Id = 7, Name = "Invalido", Description = "Lead com dados inconsistentes.", DisplayOrder = 7, IsActive = true }
    ];

    private static LeadSource[] GetSources() =>
    [
        new LeadSource { Id = 1, Name = "Cadastro manual", Description = "Lead inserido manualmente no painel.", Url = null, IsActive = true },
        new LeadSource { Id = 2, Name = "Importacao CSV", Description = "Lead importado por arquivo CSV.", Url = null, IsActive = true },
        new LeadSource { Id = 3, Name = "Google Maps", Description = "Lead identificado a partir de busca publica.", Url = "https://www.google.com/maps", IsActive = true },
        new LeadSource { Id = 4, Name = "Site proprio", Description = "Lead captado via site do profissional.", Url = null, IsActive = true },
        new LeadSource { Id = 5, Name = "Indicacao", Description = "Lead recebido por indicacao.", Url = null, IsActive = true },
        new LeadSource { Id = 6, Name = "Outro", Description = "Outras fontes publicas ou privadas.", Url = null, IsActive = true },
        new LeadSource { Id = 7, Name = "OLX", Description = "Lead captado em anuncios publicos da OLX.", Url = "https://www.olx.com.br", IsActive = true },
        new LeadSource { Id = 8, Name = "Telelistas", Description = "Lead captado no guia Telelistas.", Url = "https://www.telelistas.net", IsActive = true },
        new LeadSource { Id = 9, Name = "GuiaMais", Description = "Lead captado no guia GuiaMais.", Url = "https://www.guiamais.com.br", IsActive = true }
    ];

    private static Region[] GetRegions() =>
    [
        new Region { Id = 1, State = "SP", City = "Sao Paulo", Neighborhood = "Tatuape", ZipCode = "03301000", Zone = "Leste", Latitude = -23.540100m, Longitude = -46.576400m, RadiusKm = 5m, IsActive = true },
        new Region { Id = 2, State = "SP", City = "Sao Paulo", Neighborhood = "Mooca", ZipCode = "03104000", Zone = "Leste", Latitude = -23.556200m, Longitude = -46.601700m, RadiusKm = 5m, IsActive = true },
        new Region { Id = 3, State = "SP", City = "Campinas", Neighborhood = "Cambui", ZipCode = "13024000", Zone = "Central", Latitude = -22.891400m, Longitude = -47.060800m, RadiusKm = 8m, IsActive = true },
        new Region { Id = 4, State = "SP", City = "Santo Andre", Neighborhood = "Centro", ZipCode = "09015000", Zone = "ABC", Latitude = -23.663900m, Longitude = -46.538300m, RadiusKm = 6m, IsActive = true }
    ];

    private static Professional[] GetProfessionals() =>
    [
        new Professional
        {
            Id = 1,
            FullName = "Carlos Henrique Silva",
            BusinessName = "CH Eletrica",
            Phone = "11987654321",
            WhatsApp = "11987654321",
            Email = "carlos@cheletrica.com.br",
            ProfessionId = 1,
            SourceId = 1,
            StatusId = 4,
            Notes = "Atende residencias e pequenos comercios.",
            Website = "https://cheletrica.example.com",
            Instagram = "@cheletrica",
            IsAutonomous = true,
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 2, 3, 15, 30, 0, DateTimeKind.Utc)
        },
        new Professional
        {
            Id = 2,
            FullName = "Mariana Souza",
            BusinessName = "MS Hidraulica",
            Phone = "11981234567",
            WhatsApp = "11981234567",
            Email = "mariana@hidraulicaexemplo.com.br",
            ProfessionId = 2,
            SourceId = 3,
            StatusId = 3,
            Notes = "Especialista em reparos rapidos.",
            Instagram = "@mshidraulica",
            IsAutonomous = true,
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 12, 14, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 2, 5, 11, 0, 0, DateTimeKind.Utc)
        },
        new Professional
        {
            Id = 3,
            FullName = "Roberto Lima",
            BusinessName = "Pinturas Lima",
            Phone = "19999887766",
            Email = "roberto@pinturaslima.com.br",
            ProfessionId = 3,
            SourceId = 2,
            StatusId = 1,
            Notes = "Orcamentos por foto e visita tecnica.",
            Website = "https://pinturaslima.example.com",
            IsAutonomous = true,
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 15, 9, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 20, 10, 30, 0, DateTimeKind.Utc)
        }
    ];

    private static ProfessionalRegion[] GetProfessionalRegions() =>
    [
        new ProfessionalRegion { Id = 1, ProfessionalId = 1, RegionId = 1, ConfidenceLevel = "Alta", IsPrimaryRegion = true },
        new ProfessionalRegion { Id = 2, ProfessionalId = 1, RegionId = 2, ConfidenceLevel = "Media", IsPrimaryRegion = false },
        new ProfessionalRegion { Id = 3, ProfessionalId = 2, RegionId = 2, ConfidenceLevel = "Alta", IsPrimaryRegion = true },
        new ProfessionalRegion { Id = 4, ProfessionalId = 2, RegionId = 4, ConfidenceLevel = "Media", IsPrimaryRegion = false },
        new ProfessionalRegion { Id = 5, ProfessionalId = 3, RegionId = 3, ConfidenceLevel = "Alta", IsPrimaryRegion = true }
    ];

    private static Interaction[] GetInteractions() =>
    [
        new Interaction
        {
            Id = 1,
            ProfessionalId = 1,
            InteractionType = InteractionType.WhatsApp,
            Description = "Primeiro contato realizado, respondeu rapidamente.",
            InteractionDate = new DateTime(2026, 2, 1, 14, 0, 0, DateTimeKind.Utc),
            CreatedBy = "Admin"
        },
        new Interaction
        {
            Id = 2,
            ProfessionalId = 1,
            InteractionType = InteractionType.PhoneCall,
            Description = "Agendou retorno para a proxima semana.",
            InteractionDate = new DateTime(2026, 2, 3, 9, 30, 0, DateTimeKind.Utc),
            CreatedBy = "Admin"
        },
        new Interaction
        {
            Id = 3,
            ProfessionalId = 2,
            InteractionType = InteractionType.Email,
            Description = "Enviou portfolio e tabela de precos.",
            InteractionDate = new DateTime(2026, 2, 5, 11, 0, 0, DateTimeKind.Utc),
            CreatedBy = "Admin"
        }
    ];

    private static Evidence[] GetEvidences() =>
    [
        new Evidence
        {
            Id = 1,
            ProfessionalId = 1,
            FieldName = "Telefone",
            FieldValue = "11 98765-4321",
            SourceUrl = "https://maps.example.com/cheletrica",
            CollectedAt = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc)
        },
        new Evidence
        {
            Id = 2,
            ProfessionalId = 2,
            FieldName = "Instagram",
            FieldValue = "@mshidraulica",
            SourceUrl = "https://instagram.com/mshidraulica",
            CollectedAt = new DateTime(2026, 1, 15, 13, 0, 0, DateTimeKind.Utc)
        }
    ];

    private static LeadCaptureRun[] GetLeadCaptureRuns() =>
    [
        new LeadCaptureRun
        {
            Id = 1,
            LeadSourceId = 2,
            CaptureType = "CsvImport",
            Status = "PendingImplementation",
            FileName = "eletricistas-sp.csv",
            Notes = "Importacao futura via CSV.",
            CreatedBy = "Seeder",
            StartedAt = new DateTime(2026, 1, 2, 8, 0, 0, DateTimeKind.Utc),
            CompletedAt = new DateTime(2026, 1, 2, 8, 0, 0, DateTimeKind.Utc),
            CreatedAt = new DateTime(2026, 1, 2, 8, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 2, 8, 0, 0, DateTimeKind.Utc)
        }
    ];

    private static AppSetting[] GetAppSettings()
    {
        var createdAt = new DateTime(2026, 3, 7, 12, 0, 0, DateTimeKind.Utc);

        return
        [
            new AppSetting
            {
                Id = 1,
                Key = "crawler.concurrent_workers",
                Category = "Crawler",
                DisplayName = "Workers concorrentes",
                Description = "Quantidade maxima de workers assincros para o scheduler.",
                DataType = "int",
                Value = "4",
                DefaultValue = "4",
                IsSensitive = false,
                IsEditable = true,
                DisplayOrder = 1,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new AppSetting
            {
                Id = 2,
                Key = "crawler.http_concurrency",
                Category = "Crawler",
                DisplayName = "Concorrencia HTTP",
                Description = "Limite de requisicoes HTTP simultaneas.",
                DataType = "int",
                Value = "12",
                DefaultValue = "12",
                IsSensitive = false,
                IsEditable = true,
                DisplayOrder = 2,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new AppSetting
            {
                Id = 3,
                Key = "crawler.http_timeout_seconds",
                Category = "Crawler",
                DisplayName = "Timeout HTTP (segundos)",
                Description = "Timeout padrao das requisicoes HTTP.",
                DataType = "int",
                Value = "30",
                DefaultValue = "30",
                IsSensitive = false,
                IsEditable = true,
                DisplayOrder = 3,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new AppSetting
            {
                Id = 4,
                Key = "crawler.max_retries",
                Category = "Crawler",
                DisplayName = "Maximo de tentativas",
                Description = "Numero maximo de retries por requisicao.",
                DataType = "int",
                Value = "3",
                DefaultValue = "3",
                IsSensitive = false,
                IsEditable = true,
                DisplayOrder = 4,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new AppSetting
            {
                Id = 5,
                Key = "crawler.request_delay_ms",
                Category = "Crawler",
                DisplayName = "Delay entre requisicoes (ms)",
                Description = "Intervalo minimo entre chamadas para a mesma fonte.",
                DataType = "int",
                Value = "700",
                DefaultValue = "700",
                IsSensitive = false,
                IsEditable = true,
                DisplayOrder = 5,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new AppSetting
            {
                Id = 6,
                Key = "crawler.browser_fallback_enabled",
                Category = "Crawler",
                DisplayName = "Fallback de navegador",
                Description = "Permite trocar de HTTP para browser automation quando a fonte for dinamica.",
                DataType = "bool",
                Value = "true",
                DefaultValue = "true",
                IsSensitive = false,
                IsEditable = true,
                DisplayOrder = 6,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new AppSetting
            {
                Id = 7,
                Key = "crawler.user_agent_rotation_enabled",
                Category = "Crawler",
                DisplayName = "Rotacao de user-agent",
                Description = "Habilita rotacao simples de user-agents.",
                DataType = "bool",
                Value = "true",
                DefaultValue = "true",
                IsSensitive = false,
                IsEditable = true,
                DisplayOrder = 7,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new AppSetting
            {
                Id = 8,
                Key = "crawler.proxy_list_json",
                Category = "Crawler",
                DisplayName = "Lista de proxies",
                Description = "JSON com proxies opcionais para rotacao.",
                DataType = "json",
                Value = "[]",
                DefaultValue = "[]",
                IsSensitive = false,
                IsEditable = true,
                DisplayOrder = 8,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new AppSetting
            {
                Id = 9,
                Key = "crawler.max_pages_per_target",
                Category = "Crawler",
                DisplayName = "Maximo de paginas por alvo",
                Description = "Limite de paginas a percorrer por site e alvo.",
                DataType = "int",
                Value = "50",
                DefaultValue = "50",
                IsSensitive = false,
                IsEditable = true,
                DisplayOrder = 9,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new AppSetting
            {
                Id = 10,
                Key = "crawler.max_records_per_run",
                Category = "Crawler",
                DisplayName = "Maximo de leads por execucao",
                Description = "Quantidade maxima de registros persistidos por execucao.",
                DataType = "int",
                Value = "5000",
                DefaultValue = "5000",
                IsSensitive = false,
                IsEditable = true,
                DisplayOrder = 10,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new AppSetting
            {
                Id = 11,
                Key = "crawler.google_maps.max_idle_scrolls",
                Category = "GoogleMaps",
                DisplayName = "Scrolls ociosos maximos",
                Description = "Limite de scrolls sem novos cards no Google Maps.",
                DataType = "int",
                Value = "8",
                DefaultValue = "8",
                IsSensitive = false,
                IsEditable = true,
                DisplayOrder = 1,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new AppSetting
            {
                Id = 12,
                Key = "crawler.google_maps.scroll_pause_ms",
                Category = "GoogleMaps",
                DisplayName = "Pausa do scroll (ms)",
                Description = "Pausa entre scrolls da lista do Google Maps.",
                DataType = "int",
                Value = "1500",
                DefaultValue = "1500",
                IsSensitive = false,
                IsEditable = true,
                DisplayOrder = 2,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new AppSetting
            {
                Id = 13,
                Key = "crawler.browser_headless",
                Category = "Crawler",
                DisplayName = "Executar navegadores em headless",
                Description = "Controla se Selenium e Playwright rodam em modo headless.",
                DataType = "bool",
                Value = "true",
                DefaultValue = "true",
                IsSensitive = false,
                IsEditable = true,
                DisplayOrder = 11,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            }
        ];
    }
}
