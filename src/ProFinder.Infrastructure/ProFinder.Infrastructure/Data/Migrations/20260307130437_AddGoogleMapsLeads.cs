using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProFinder.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleMapsLeads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "prf_lead_capture_runs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemsCaptured",
                table: "prf_lead_capture_runs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ItemsInserted",
                table: "prf_lead_capture_runs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ItemsSkipped",
                table: "prf_lead_capture_runs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ItemsUpdated",
                table: "prf_lead_capture_runs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SearchQuery",
                table: "prf_lead_capture_runs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "prf_google_maps_leads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadCaptureRunId = table.Column<int>(type: "int", nullable: false),
                    LeadSourceId = table.Column<int>(type: "int", nullable: false),
                    ProfessionId = table.Column<int>(type: "int", nullable: true),
                    RegionId = table.Column<int>(type: "int", nullable: true),
                    ImportedProfessionalId = table.Column<int>(type: "int", nullable: true),
                    SearchQuery = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PlaceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NormalizedPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Rating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    ReviewCount = table.Column<int>(type: "int", nullable: true),
                    ImportStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RawPayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScrapedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prf_google_maps_leads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prf_google_maps_leads_prf_lead_capture_runs_LeadCaptureRunId",
                        column: x => x.LeadCaptureRunId,
                        principalTable: "prf_lead_capture_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prf_google_maps_leads_prf_lead_sources_LeadSourceId",
                        column: x => x.LeadSourceId,
                        principalTable: "prf_lead_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prf_google_maps_leads_prf_professionals_ImportedProfessionalId",
                        column: x => x.ImportedProfessionalId,
                        principalTable: "prf_professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_prf_google_maps_leads_prf_professions_ProfessionId",
                        column: x => x.ProfessionId,
                        principalTable: "prf_professions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_prf_google_maps_leads_prf_regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "prf_regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "prf_lead_capture_runs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ErrorMessage", "ItemsCaptured", "ItemsInserted", "ItemsSkipped", "ItemsUpdated", "SearchQuery" },
                values: new object[] { null, 0, 0, 0, 0, null });

            migrationBuilder.CreateIndex(
                name: "IX_prf_google_maps_leads_ImportedProfessionalId",
                table: "prf_google_maps_leads",
                column: "ImportedProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_prf_google_maps_leads_LeadCaptureRunId",
                table: "prf_google_maps_leads",
                column: "LeadCaptureRunId");

            migrationBuilder.CreateIndex(
                name: "IX_prf_google_maps_leads_LeadSourceId",
                table: "prf_google_maps_leads",
                column: "LeadSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_prf_google_maps_leads_NormalizedPhone_Name",
                table: "prf_google_maps_leads",
                columns: new[] { "NormalizedPhone", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_prf_google_maps_leads_PlaceUrl",
                table: "prf_google_maps_leads",
                column: "PlaceUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prf_google_maps_leads_ProfessionId",
                table: "prf_google_maps_leads",
                column: "ProfessionId");

            migrationBuilder.CreateIndex(
                name: "IX_prf_google_maps_leads_RegionId",
                table: "prf_google_maps_leads",
                column: "RegionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prf_google_maps_leads");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "prf_lead_capture_runs");

            migrationBuilder.DropColumn(
                name: "ItemsCaptured",
                table: "prf_lead_capture_runs");

            migrationBuilder.DropColumn(
                name: "ItemsInserted",
                table: "prf_lead_capture_runs");

            migrationBuilder.DropColumn(
                name: "ItemsSkipped",
                table: "prf_lead_capture_runs");

            migrationBuilder.DropColumn(
                name: "ItemsUpdated",
                table: "prf_lead_capture_runs");

            migrationBuilder.DropColumn(
                name: "SearchQuery",
                table: "prf_lead_capture_runs");
        }
    }
}
