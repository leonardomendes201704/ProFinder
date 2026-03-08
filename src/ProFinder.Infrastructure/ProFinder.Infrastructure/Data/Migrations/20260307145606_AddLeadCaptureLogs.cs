using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProFinder.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadCaptureLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prf_app_settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DefaultValue = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsSensitive = table.Column<bool>(type: "bit", nullable: false),
                    IsEditable = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prf_app_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prf_lead_capture_logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadCaptureRunId = table.Column<int>(type: "int", nullable: false),
                    LogLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prf_lead_capture_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prf_lead_capture_logs_prf_lead_capture_runs_LeadCaptureRunId",
                        column: x => x.LeadCaptureRunId,
                        principalTable: "prf_lead_capture_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prf_provider_leads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadCaptureRunId = table.Column<int>(type: "int", nullable: false),
                    LeadSourceId = table.Column<int>(type: "int", nullable: false),
                    ProfessionId = table.Column<int>(type: "int", nullable: true),
                    RegionId = table.Column<int>(type: "int", nullable: true),
                    ImportedProfessionalId = table.Column<int>(type: "int", nullable: true),
                    SiteKey = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SearchQuery = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DeduplicationKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    WhatsApp = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NormalizedPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Neighborhood = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    City = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    State = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SourceListingUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceDetailsUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ImportStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SourceSitesJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SourceUrlsJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    SourceCount = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ReviewCount = table.Column<int>(type: "int", nullable: true),
                    RawPayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScrapedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prf_provider_leads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prf_provider_leads_prf_lead_capture_runs_LeadCaptureRunId",
                        column: x => x.LeadCaptureRunId,
                        principalTable: "prf_lead_capture_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prf_provider_leads_prf_lead_sources_LeadSourceId",
                        column: x => x.LeadSourceId,
                        principalTable: "prf_lead_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prf_provider_leads_prf_professionals_ImportedProfessionalId",
                        column: x => x.ImportedProfessionalId,
                        principalTable: "prf_professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prf_provider_leads_prf_professions_ProfessionId",
                        column: x => x.ProfessionId,
                        principalTable: "prf_professions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prf_provider_leads_prf_regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "prf_regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "prf_app_settings",
                columns: new[] { "Id", "Category", "CreatedAt", "DataType", "DefaultValue", "Description", "DisplayName", "DisplayOrder", "IsEditable", "IsSensitive", "Key", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { 1, "Crawler", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "int", "4", "Quantidade maxima de workers assincros para o scheduler.", "Workers concorrentes", 1, true, false, "crawler.concurrent_workers", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "4" },
                    { 2, "Crawler", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "int", "12", "Limite de requisicoes HTTP simultaneas.", "Concorrencia HTTP", 2, true, false, "crawler.http_concurrency", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "12" },
                    { 3, "Crawler", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "int", "30", "Timeout padrao das requisicoes HTTP.", "Timeout HTTP (segundos)", 3, true, false, "crawler.http_timeout_seconds", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "30" },
                    { 4, "Crawler", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "int", "3", "Numero maximo de retries por requisicao.", "Maximo de tentativas", 4, true, false, "crawler.max_retries", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "3" },
                    { 5, "Crawler", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "int", "700", "Intervalo minimo entre chamadas para a mesma fonte.", "Delay entre requisicoes (ms)", 5, true, false, "crawler.request_delay_ms", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "700" },
                    { 6, "Crawler", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "bool", "true", "Permite trocar de HTTP para browser automation quando a fonte for dinamica.", "Fallback de navegador", 6, true, false, "crawler.browser_fallback_enabled", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "true" },
                    { 7, "Crawler", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "bool", "true", "Habilita rotacao simples de user-agents.", "Rotacao de user-agent", 7, true, false, "crawler.user_agent_rotation_enabled", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "true" },
                    { 8, "Crawler", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "json", "[]", "JSON com proxies opcionais para rotacao.", "Lista de proxies", 8, true, false, "crawler.proxy_list_json", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "[]" },
                    { 9, "Crawler", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "int", "50", "Limite de paginas a percorrer por site e alvo.", "Maximo de paginas por alvo", 9, true, false, "crawler.max_pages_per_target", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "50" },
                    { 10, "Crawler", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "int", "5000", "Quantidade maxima de registros persistidos por execucao.", "Maximo de leads por execucao", 10, true, false, "crawler.max_records_per_run", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "5000" },
                    { 11, "GoogleMaps", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "int", "8", "Limite de scrolls sem novos cards no Google Maps.", "Scrolls ociosos maximos", 1, true, false, "crawler.google_maps.max_idle_scrolls", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "8" },
                    { 12, "GoogleMaps", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "int", "1500", "Pausa entre scrolls da lista do Google Maps.", "Pausa do scroll (ms)", 2, true, false, "crawler.google_maps.scroll_pause_ms", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "1500" },
                    { 13, "Crawler", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "bool", "true", "Controla se Selenium e Playwright rodam em modo headless.", "Executar navegadores em headless", 11, true, false, "crawler.browser_headless", new DateTime(2026, 3, 7, 12, 0, 0, 0, DateTimeKind.Utc), "true" }
                });

            migrationBuilder.UpdateData(
                table: "prf_interactions",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Agendou retorno para a proxima semana.");

            migrationBuilder.UpdateData(
                table: "prf_interactions",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Enviou portfolio e tabela de precos.");

            migrationBuilder.UpdateData(
                table: "prf_lead_capture_runs",
                keyColumn: "Id",
                keyValue: 1,
                column: "Notes",
                value: "Importacao futura via CSV.");

            migrationBuilder.UpdateData(
                table: "prf_lead_sources",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Importacao CSV");

            migrationBuilder.UpdateData(
                table: "prf_lead_sources",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Lead identificado a partir de busca publica.");

            migrationBuilder.UpdateData(
                table: "prf_lead_sources",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Site proprio");

            migrationBuilder.UpdateData(
                table: "prf_lead_sources",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Lead recebido por indicacao.", "Indicacao" });

            migrationBuilder.UpdateData(
                table: "prf_lead_sources",
                keyColumn: "Id",
                keyValue: 6,
                column: "Description",
                value: "Outras fontes publicas ou privadas.");

            migrationBuilder.InsertData(
                table: "prf_lead_sources",
                columns: new[] { "Id", "Description", "IsActive", "Name", "Url" },
                values: new object[,]
                {
                    { 7, "Lead captado em anuncios publicos da OLX.", true, "OLX", "https://www.olx.com.br" },
                    { 8, "Lead captado no guia Telelistas.", true, "Telelistas", "https://www.telelistas.net" },
                    { 9, "Lead captado no guia GuiaMais.", true, "GuiaMais", "https://www.guiamais.com.br" }
                });

            migrationBuilder.UpdateData(
                table: "prf_lead_statuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Lead recem-cadastrado.");

            migrationBuilder.UpdateData(
                table: "prf_lead_statuses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Lead em validacao.", "Em analise" });

            migrationBuilder.UpdateData(
                table: "prf_lead_statuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Lead com criterios minimos atendidos.");

            migrationBuilder.UpdateData(
                table: "prf_lead_statuses",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Nao interessado");

            migrationBuilder.UpdateData(
                table: "prf_lead_statuses",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Invalido");

            migrationBuilder.UpdateData(
                table: "prf_professional_regions",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConfidenceLevel",
                value: "Media");

            migrationBuilder.UpdateData(
                table: "prf_professional_regions",
                keyColumn: "Id",
                keyValue: 4,
                column: "ConfidenceLevel",
                value: "Media");

            migrationBuilder.UpdateData(
                table: "prf_professionals",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BusinessName", "Notes" },
                values: new object[] { "CH Eletrica", "Atende residencias e pequenos comercios." });

            migrationBuilder.UpdateData(
                table: "prf_professionals",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BusinessName", "Notes" },
                values: new object[] { "MS Hidraulica", "Especialista em reparos rapidos." });

            migrationBuilder.UpdateData(
                table: "prf_professionals",
                keyColumn: "Id",
                keyValue: 3,
                column: "Notes",
                value: "Orcamentos por foto e visita tecnica.");

            migrationBuilder.UpdateData(
                table: "prf_professions",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Profissionais de instalacoes e manutencao eletrica.");

            migrationBuilder.UpdateData(
                table: "prf_professions",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Profissionais de hidraulica e reparos em geral.");

            migrationBuilder.UpdateData(
                table: "prf_professions",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "Profissionais especializados em abertura e copia de chaves.");

            migrationBuilder.UpdateData(
                table: "prf_regions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "City", "Neighborhood" },
                values: new object[] { "Sao Paulo", "Tatuape" });

            migrationBuilder.UpdateData(
                table: "prf_regions",
                keyColumn: "Id",
                keyValue: 2,
                column: "City",
                value: "Sao Paulo");

            migrationBuilder.UpdateData(
                table: "prf_regions",
                keyColumn: "Id",
                keyValue: 3,
                column: "Neighborhood",
                value: "Cambui");

            migrationBuilder.UpdateData(
                table: "prf_regions",
                keyColumn: "Id",
                keyValue: 4,
                column: "City",
                value: "Santo Andre");

            migrationBuilder.CreateIndex(
                name: "IX_prf_app_settings_Category_DisplayOrder",
                table: "prf_app_settings",
                columns: new[] { "Category", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_prf_app_settings_Key",
                table: "prf_app_settings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prf_lead_capture_logs_CreatedAt",
                table: "prf_lead_capture_logs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_prf_lead_capture_logs_LeadCaptureRunId",
                table: "prf_lead_capture_logs",
                column: "LeadCaptureRunId");

            migrationBuilder.CreateIndex(
                name: "IX_prf_provider_leads_City",
                table: "prf_provider_leads",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_prf_provider_leads_DeduplicationKey",
                table: "prf_provider_leads",
                column: "DeduplicationKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prf_provider_leads_ImportedProfessionalId",
                table: "prf_provider_leads",
                column: "ImportedProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_prf_provider_leads_ImportStatus",
                table: "prf_provider_leads",
                column: "ImportStatus");

            migrationBuilder.CreateIndex(
                name: "IX_prf_provider_leads_LeadCaptureRunId",
                table: "prf_provider_leads",
                column: "LeadCaptureRunId");

            migrationBuilder.CreateIndex(
                name: "IX_prf_provider_leads_LeadSourceId",
                table: "prf_provider_leads",
                column: "LeadSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_prf_provider_leads_NormalizedPhone",
                table: "prf_provider_leads",
                column: "NormalizedPhone");

            migrationBuilder.CreateIndex(
                name: "IX_prf_provider_leads_ProfessionId",
                table: "prf_provider_leads",
                column: "ProfessionId");

            migrationBuilder.CreateIndex(
                name: "IX_prf_provider_leads_RegionId",
                table: "prf_provider_leads",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_prf_provider_leads_SiteKey",
                table: "prf_provider_leads",
                column: "SiteKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prf_app_settings");

            migrationBuilder.DropTable(
                name: "prf_lead_capture_logs");

            migrationBuilder.DropTable(
                name: "prf_provider_leads");

            migrationBuilder.DeleteData(
                table: "prf_lead_sources",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "prf_lead_sources",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "prf_lead_sources",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.UpdateData(
                table: "prf_interactions",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Agendou retorno para a próxima semana.");

            migrationBuilder.UpdateData(
                table: "prf_interactions",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Enviou portfólio e tabela de preços.");

            migrationBuilder.UpdateData(
                table: "prf_lead_capture_runs",
                keyColumn: "Id",
                keyValue: 1,
                column: "Notes",
                value: "Importação futura via CSV.");

            migrationBuilder.UpdateData(
                table: "prf_lead_sources",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Importação CSV");

            migrationBuilder.UpdateData(
                table: "prf_lead_sources",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Lead identificado a partir de busca pública.");

            migrationBuilder.UpdateData(
                table: "prf_lead_sources",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Site próprio");

            migrationBuilder.UpdateData(
                table: "prf_lead_sources",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Lead recebido por indicação.", "Indicação" });

            migrationBuilder.UpdateData(
                table: "prf_lead_sources",
                keyColumn: "Id",
                keyValue: 6,
                column: "Description",
                value: "Outras fontes públicas ou privadas.");

            migrationBuilder.UpdateData(
                table: "prf_lead_statuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Lead recém-cadastrado.");

            migrationBuilder.UpdateData(
                table: "prf_lead_statuses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Lead em validação.", "Em análise" });

            migrationBuilder.UpdateData(
                table: "prf_lead_statuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Lead com critérios mínimos atendidos.");

            migrationBuilder.UpdateData(
                table: "prf_lead_statuses",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Não interessado");

            migrationBuilder.UpdateData(
                table: "prf_lead_statuses",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Inválido");

            migrationBuilder.UpdateData(
                table: "prf_professional_regions",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConfidenceLevel",
                value: "Média");

            migrationBuilder.UpdateData(
                table: "prf_professional_regions",
                keyColumn: "Id",
                keyValue: 4,
                column: "ConfidenceLevel",
                value: "Média");

            migrationBuilder.UpdateData(
                table: "prf_professionals",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BusinessName", "Notes" },
                values: new object[] { "CH Elétrica", "Atende residências e pequenos comércios." });

            migrationBuilder.UpdateData(
                table: "prf_professionals",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BusinessName", "Notes" },
                values: new object[] { "MS Hidráulica", "Especialista em reparos rápidos." });

            migrationBuilder.UpdateData(
                table: "prf_professionals",
                keyColumn: "Id",
                keyValue: 3,
                column: "Notes",
                value: "Orçamentos por foto e visita técnica.");

            migrationBuilder.UpdateData(
                table: "prf_professions",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Profissionais de instalações e manutenção elétrica.");

            migrationBuilder.UpdateData(
                table: "prf_professions",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Profissionais de hidráulica e reparos em geral.");

            migrationBuilder.UpdateData(
                table: "prf_professions",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "Profissionais especializados em abertura e cópia de chaves.");

            migrationBuilder.UpdateData(
                table: "prf_regions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "City", "Neighborhood" },
                values: new object[] { "São Paulo", "Tatuapé" });

            migrationBuilder.UpdateData(
                table: "prf_regions",
                keyColumn: "Id",
                keyValue: 2,
                column: "City",
                value: "São Paulo");

            migrationBuilder.UpdateData(
                table: "prf_regions",
                keyColumn: "Id",
                keyValue: 3,
                column: "Neighborhood",
                value: "Cambuí");

            migrationBuilder.UpdateData(
                table: "prf_regions",
                keyColumn: "Id",
                keyValue: 4,
                column: "City",
                value: "Santo André");
        }
    }
}
