using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KuendaFinance.Operations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLatePenalties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Penalty",
                table: "Installments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Penalty",
                table: "Installments");
        }
    }
}
