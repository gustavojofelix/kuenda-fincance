using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KuendaFinance.Operations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyPenaltyRateToLoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DailyPenaltyRate",
                table: "Loans",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyPenaltyRate",
                table: "Loans");
        }
    }
}
