using FluentAssertions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.IntegrationTests.Abstractions;
using ViaRiceco.Modules.Accounting.Application.Incomes.UpdateIncome;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.GetSettlementPeriod;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Budgets.Application.Expenses.GetExpenses;
using ViaRiceco.Modules.Budgets.Application.Expenses.Models;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Portfolios.Application.Currencies.Models;
using ViaRiceco.Modules.Portfolios.Application.FinancialGoals.Models;
using ViaRiceco.Modules.Portfolios.Application.FinancialGoals.UpdateFinancialGoal;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.Models;
using SettlementPeriodDto = ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models.SettlementPeriodDto;
using IncomeDto = ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models.IncomeDto;
using TaxDto = ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models.TaxDto;

namespace ViaRiceco.IntegrationTests;

/// <summary>
/// Complex integration tests that span multiple modules and test realistic financial workflows
/// </summary>
public sealed class ComplexFinancialWorkflowTests : BaseIntegrationTest
{
    public ComplexFinancialWorkflowTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_SimulateIncomeFluctuation_AndAdjustFinancialStrategy()
    {
        // Arrange - Create base settlement period with good income
        SettlementPeriodDto baseSettlement = await Sender.CreateSettlementPeriodAsync(month: 6, year: 2024);
        IncomeDto initialIncome = await Sender.AddIncomeToSettlementPeriodAsync(baseSettlement.Id, 8000);
        
        TaxTypeDto taxType = await Sender.CreateTaxTypeAsync("Progressive Income Tax");
        await Sender.AddTaxToSettlementPeriodAsync(baseSettlement.Id, taxType.Id, 1600);

        // Create initial financial goal based on high income
        FinancialGoalDto originalGoal = await Sender.CreateFinancialGoalAsync(
            name: "High Income Investment Goal");

        // Act - Simulate income reduction (economic downturn)
        var updatedIncomeCommand = new UpdateIncomeCommand(baseSettlement.Id, initialIncome.Id, 4000);
        Result<IncomeDto> updatedIncomeResult = await Sender.Send(updatedIncomeCommand);

        // Verify the updated settlement period reflects new calculations
        var getSettlementQuery = new GetSettlementPeriodQuery(baseSettlement.Id);
        Result<SettlementPeriodDto> updatedSettlementResult = await Sender.Send(getSettlementQuery);

        // Adjust financial goal based on reduced income
        var adjustedGoalCommand = new UpdateFinancialGoalCommand(
            originalGoal.Id,
            "Adjusted Investment Goal", // New realistic name
            originalGoal.ParentId);
        Result<FinancialGoalDto> adjustedGoalResult = await Sender.Send(adjustedGoalCommand);

        // Assert - Verify adaptation to income changes
        updatedIncomeResult.IsSuccess.Should().BeTrue();
        updatedIncomeResult.Value.Value.Should().Be(4000);

        updatedSettlementResult.IsSuccess.Should().BeTrue();
        updatedSettlementResult.Value.TotalIncome.Should().Be(4000);
        updatedSettlementResult.Value.NetAmount.Should().Be(2400); // 4000 - 1600

        adjustedGoalResult.IsSuccess.Should().BeTrue();
        adjustedGoalResult.Value.Name.Should().Be("Adjusted Investment Goal");
        
        // Verify basic data integrity after adjustment
        decimal monthlyNetIncome = updatedSettlementResult.Value.NetAmount;
        monthlyNetIncome.Should().Be(2400); // 4000 - 1600
    }
}
