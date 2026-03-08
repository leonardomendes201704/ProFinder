using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProFinder.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prf_lead_sources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prf_lead_sources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prf_lead_statuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prf_lead_statuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prf_professions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prf_professions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prf_regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    State = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Neighborhood = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Zone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    RadiusKm = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prf_regions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prf_lead_capture_runs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadSourceId = table.Column<int>(type: "int", nullable: true),
                    CaptureType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prf_lead_capture_runs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prf_lead_capture_runs_prf_lead_sources_LeadSourceId",
                        column: x => x.LeadSourceId,
                        principalTable: "prf_lead_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "prf_professionals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BusinessName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    WhatsApp = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DocumentNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ProfessionId = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Instagram = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsAutonomous = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prf_professionals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prf_professionals_prf_lead_sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "prf_lead_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prf_professionals_prf_lead_statuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "prf_lead_statuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prf_professionals_prf_professions_ProfessionId",
                        column: x => x.ProfessionId,
                        principalTable: "prf_professions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prf_evidences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfessionalId = table.Column<int>(type: "int", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FieldValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SourceUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CollectedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prf_evidences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prf_evidences_prf_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "prf_professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prf_interactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfessionalId = table.Column<int>(type: "int", nullable: false),
                    InteractionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    InteractionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prf_interactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prf_interactions_prf_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "prf_professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prf_professional_regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfessionalId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    ConfidenceLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsPrimaryRegion = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prf_professional_regions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prf_professional_regions_prf_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "prf_professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prf_professional_regions_prf_regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "prf_regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "prf_lead_sources",
                columns: new[] { "Id", "Description", "IsActive", "Name", "Url" },
                values: new object[,]
                {
                    { 1, "Lead inserido manualmente no painel.", true, "Cadastro manual", null },
                    { 2, "Lead importado por arquivo CSV.", true, "Importação CSV", null },
                    { 3, "Lead identificado a partir de busca pública.", true, "Google Maps", "https://www.google.com/maps" },
                    { 4, "Lead captado via site do profissional.", true, "Site próprio", null },
                    { 5, "Lead recebido por indicação.", true, "Indicação", null },
                    { 6, "Outras fontes públicas ou privadas.", true, "Outro", null }
                });

            migrationBuilder.InsertData(
                table: "prf_lead_statuses",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Lead recém-cadastrado.", 1, true, "Novo" },
                    { 2, "Lead em validação.", 2, true, "Em análise" },
                    { 3, "Lead com critérios mínimos atendidos.", 3, true, "Qualificado" },
                    { 4, "Primeiro contato realizado.", 4, true, "Contatado" },
                    { 5, "Lead demonstrou interesse.", 5, true, "Interessado" },
                    { 6, "Lead sem interesse no momento.", 6, true, "Não interessado" },
                    { 7, "Lead com dados inconsistentes.", 7, true, "Inválido" }
                });

            migrationBuilder.InsertData(
                table: "prf_professions",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Profissionais de instalações e manutenção elétrica.", true, "Eletricista" },
                    { 2, "Profissionais de hidráulica e reparos em geral.", true, "Encanador" },
                    { 3, "Profissionais de pintura residencial e comercial.", true, "Pintor" },
                    { 4, "Profissionais especializados em abertura e cópia de chaves.", true, "Chaveiro" }
                });

            migrationBuilder.InsertData(
                table: "prf_regions",
                columns: new[] { "Id", "City", "IsActive", "Latitude", "Longitude", "Neighborhood", "RadiusKm", "State", "ZipCode", "Zone" },
                values: new object[,]
                {
                    { 1, "São Paulo", true, -23.540100m, -46.576400m, "Tatuapé", 5m, "SP", "03301000", "Leste" },
                    { 2, "São Paulo", true, -23.556200m, -46.601700m, "Mooca", 5m, "SP", "03104000", "Leste" },
                    { 3, "Campinas", true, -22.891400m, -47.060800m, "Cambuí", 8m, "SP", "13024000", "Central" },
                    { 4, "Santo André", true, -23.663900m, -46.538300m, "Centro", 6m, "SP", "09015000", "ABC" }
                });

            migrationBuilder.InsertData(
                table: "prf_lead_capture_runs",
                columns: new[] { "Id", "CaptureType", "CompletedAt", "CreatedAt", "CreatedBy", "FileName", "LeadSourceId", "Notes", "SourceUrl", "StartedAt", "Status", "UpdatedAt" },
                values: new object[] { 1, "CsvImport", new DateTime(2026, 1, 2, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 2, 8, 0, 0, 0, DateTimeKind.Utc), "Seeder", "eletricistas-sp.csv", 2, "Importação futura via CSV.", null, new DateTime(2026, 1, 2, 8, 0, 0, 0, DateTimeKind.Utc), "PendingImplementation", new DateTime(2026, 1, 2, 8, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "prf_professionals",
                columns: new[] { "Id", "BusinessName", "CreatedAt", "DocumentNumber", "Email", "FullName", "Instagram", "IsActive", "IsAutonomous", "Notes", "Phone", "ProfessionId", "SourceId", "StatusId", "UpdatedAt", "Website", "WhatsApp" },
                values: new object[,]
                {
                    { 1, "CH Elétrica", new DateTime(2026, 1, 10, 12, 0, 0, 0, DateTimeKind.Utc), null, "carlos@cheletrica.com.br", "Carlos Henrique Silva", "@cheletrica", true, true, "Atende residências e pequenos comércios.", "11987654321", 1, 1, 4, new DateTime(2026, 2, 3, 15, 30, 0, 0, DateTimeKind.Utc), "https://cheletrica.example.com", "11987654321" },
                    { 2, "MS Hidráulica", new DateTime(2026, 1, 12, 14, 0, 0, 0, DateTimeKind.Utc), null, "mariana@hidraulicaexemplo.com.br", "Mariana Souza", "@mshidraulica", true, true, "Especialista em reparos rápidos.", "11981234567", 2, 3, 3, new DateTime(2026, 2, 5, 11, 0, 0, 0, DateTimeKind.Utc), null, "11981234567" },
                    { 3, "Pinturas Lima", new DateTime(2026, 1, 15, 9, 0, 0, 0, DateTimeKind.Utc), null, "roberto@pinturaslima.com.br", "Roberto Lima", null, true, true, "Orçamentos por foto e visita técnica.", "19999887766", 3, 2, 1, new DateTime(2026, 1, 20, 10, 30, 0, 0, DateTimeKind.Utc), "https://pinturaslima.example.com", null }
                });

            migrationBuilder.InsertData(
                table: "prf_evidences",
                columns: new[] { "Id", "CollectedAt", "FieldName", "FieldValue", "ProfessionalId", "SourceUrl" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 10, 12, 0, 0, 0, DateTimeKind.Utc), "Telefone", "11 98765-4321", 1, "https://maps.example.com/cheletrica" },
                    { 2, new DateTime(2026, 1, 15, 13, 0, 0, 0, DateTimeKind.Utc), "Instagram", "@mshidraulica", 2, "https://instagram.com/mshidraulica" }
                });

            migrationBuilder.InsertData(
                table: "prf_interactions",
                columns: new[] { "Id", "CreatedBy", "Description", "InteractionDate", "InteractionType", "ProfessionalId" },
                values: new object[,]
                {
                    { 1, "Admin", "Primeiro contato realizado, respondeu rapidamente.", new DateTime(2026, 2, 1, 14, 0, 0, 0, DateTimeKind.Utc), "WhatsApp", 1 },
                    { 2, "Admin", "Agendou retorno para a próxima semana.", new DateTime(2026, 2, 3, 9, 30, 0, 0, DateTimeKind.Utc), "PhoneCall", 1 },
                    { 3, "Admin", "Enviou portfólio e tabela de preços.", new DateTime(2026, 2, 5, 11, 0, 0, 0, DateTimeKind.Utc), "Email", 2 }
                });

            migrationBuilder.InsertData(
                table: "prf_professional_regions",
                columns: new[] { "Id", "ConfidenceLevel", "IsPrimaryRegion", "ProfessionalId", "RegionId" },
                values: new object[,]
                {
                    { 1, "Alta", true, 1, 1 },
                    { 2, "Média", false, 1, 2 },
                    { 3, "Alta", true, 2, 2 },
                    { 4, "Média", false, 2, 4 },
                    { 5, "Alta", true, 3, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_prf_evidences_ProfessionalId",
                table: "prf_evidences",
                column: "ProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_prf_interactions_ProfessionalId_InteractionDate",
                table: "prf_interactions",
                columns: new[] { "ProfessionalId", "InteractionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_prf_lead_capture_runs_LeadSourceId",
                table: "prf_lead_capture_runs",
                column: "LeadSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_prf_lead_sources_Name",
                table: "prf_lead_sources",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prf_lead_statuses_Name",
                table: "prf_lead_statuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prf_professional_regions_ProfessionalId_RegionId",
                table: "prf_professional_regions",
                columns: new[] { "ProfessionalId", "RegionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prf_professional_regions_RegionId",
                table: "prf_professional_regions",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_prf_professionals_Email",
                table: "prf_professionals",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_prf_professionals_FullName",
                table: "prf_professionals",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_prf_professionals_IsActive",
                table: "prf_professionals",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_prf_professionals_Phone",
                table: "prf_professionals",
                column: "Phone");

            migrationBuilder.CreateIndex(
                name: "IX_prf_professionals_ProfessionId",
                table: "prf_professionals",
                column: "ProfessionId");

            migrationBuilder.CreateIndex(
                name: "IX_prf_professionals_SourceId",
                table: "prf_professionals",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_prf_professionals_StatusId",
                table: "prf_professionals",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_prf_professionals_WhatsApp",
                table: "prf_professionals",
                column: "WhatsApp");

            migrationBuilder.CreateIndex(
                name: "IX_prf_professions_Name",
                table: "prf_professions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prf_regions_State_City_Neighborhood_ZipCode",
                table: "prf_regions",
                columns: new[] { "State", "City", "Neighborhood", "ZipCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prf_evidences");

            migrationBuilder.DropTable(
                name: "prf_interactions");

            migrationBuilder.DropTable(
                name: "prf_lead_capture_runs");

            migrationBuilder.DropTable(
                name: "prf_professional_regions");

            migrationBuilder.DropTable(
                name: "prf_professionals");

            migrationBuilder.DropTable(
                name: "prf_regions");

            migrationBuilder.DropTable(
                name: "prf_lead_sources");

            migrationBuilder.DropTable(
                name: "prf_lead_statuses");

            migrationBuilder.DropTable(
                name: "prf_professions");
        }
    }
}
