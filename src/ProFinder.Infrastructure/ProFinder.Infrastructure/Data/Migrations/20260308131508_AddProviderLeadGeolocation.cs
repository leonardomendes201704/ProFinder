using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProFinder.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderLeadGeolocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "prf_provider_leads",
                type: "decimal(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "prf_provider_leads",
                type: "decimal(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "prf_provider_leads");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "prf_provider_leads");
        }
    }
}
