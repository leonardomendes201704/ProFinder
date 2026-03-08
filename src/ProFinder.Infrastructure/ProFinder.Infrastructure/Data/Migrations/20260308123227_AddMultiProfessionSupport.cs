using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProFinder.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiProfessionSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prf_professional_professions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfessionalId = table.Column<int>(type: "int", nullable: false),
                    ProfessionId = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prf_professional_professions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prf_professional_professions_prf_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "prf_professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prf_professional_professions_prf_professions_ProfessionId",
                        column: x => x.ProfessionId,
                        principalTable: "prf_professions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prf_provider_lead_professions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderLeadId = table.Column<int>(type: "int", nullable: false),
                    ProfessionId = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prf_provider_lead_professions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prf_provider_lead_professions_prf_professions_ProfessionId",
                        column: x => x.ProfessionId,
                        principalTable: "prf_professions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prf_provider_lead_professions_prf_provider_leads_ProviderLeadId",
                        column: x => x.ProviderLeadId,
                        principalTable: "prf_provider_leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO prf_professional_professions
                (
                    ProfessionalId,
                    ProfessionId,
                    IsPrimary
                )
                SELECT p.Id,
                       p.ProfessionId,
                       CAST(1 AS bit)
                  FROM prf_professionals p
                 WHERE NOT EXISTS
                (
                    SELECT 1
                      FROM prf_professional_professions pp
                     WHERE pp.ProfessionalId = p.Id
                       AND pp.ProfessionId = p.ProfessionId
                );
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO prf_provider_lead_professions
                (
                    ProviderLeadId,
                    ProfessionId,
                    IsPrimary
                )
                SELECT pl.Id,
                       pl.ProfessionId,
                       CAST(1 AS bit)
                  FROM prf_provider_leads pl
                 WHERE pl.ProfessionId IS NOT NULL
                   AND NOT EXISTS
                (
                    SELECT 1
                      FROM prf_provider_lead_professions plp
                     WHERE plp.ProviderLeadId = pl.Id
                       AND plp.ProfessionId = pl.ProfessionId
                );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_prf_professional_professions_ProfessionalId_ProfessionId",
                table: "prf_professional_professions",
                columns: new[] { "ProfessionalId", "ProfessionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prf_professional_professions_ProfessionId",
                table: "prf_professional_professions",
                column: "ProfessionId");

            migrationBuilder.CreateIndex(
                name: "IX_prf_provider_lead_professions_ProfessionId",
                table: "prf_provider_lead_professions",
                column: "ProfessionId");

            migrationBuilder.CreateIndex(
                name: "IX_prf_provider_lead_professions_ProviderLeadId_ProfessionId",
                table: "prf_provider_lead_professions",
                columns: new[] { "ProviderLeadId", "ProfessionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prf_professional_professions");

            migrationBuilder.DropTable(
                name: "prf_provider_lead_professions");
        }
    }
}
