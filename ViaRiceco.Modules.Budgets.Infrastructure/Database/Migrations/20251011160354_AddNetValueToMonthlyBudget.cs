using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaRiceco.Modules.Budgets.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddNetValueToMonthlyBudget : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "net_value",
            schema: "budgets",
            table: "monthly_budgets",
            type: "numeric(18,2)",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_monthly_budgets_net_value",
            schema: "budgets",
            table: "monthly_budgets",
            column: "net_value");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_monthly_budgets_net_value",
            schema: "budgets",
            table: "monthly_budgets");

        migrationBuilder.DropColumn(
            name: "net_value",
            schema: "budgets",
            table: "monthly_budgets");
    }
}
