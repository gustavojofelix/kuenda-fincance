using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KuendaFinance.IAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubdomainToTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Subdomain",
                table: "Tenants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Subdomain",
                table: "Tenants",
                column: "Subdomain",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tenants_Subdomain",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Subdomain",
                table: "Tenants");
        }
    }
}
