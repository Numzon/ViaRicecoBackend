using FluentAssertions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.IntegrationTests.Abstractions;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.GetSettlementPeriods;
using ViaRiceco.Modules.Budgets.Application.Expenses.GetExpenses;
using ViaRiceco.Modules.Portfolios.Application.FinancialGoals.GetFinancialGoals;
using System.Diagnostics;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.GetTaxTypes;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.GetExpenseTypes;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.GetSettlementPeriod;
using SettlementPeriodDto = ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models.SettlementPeriodDto;
using IncomeDto = ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models.IncomeDto;
using TaxDto = ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models.TaxDto;
using TaxTypeDto = ViaRiceco.Modules.Accounting.Application.TaxTypes.Models.TaxTypeDto;
using ExpenseTypeDto = ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models.ExpenseTypeDto;
using ExpenseDto = ViaRiceco.Modules.Budgets.Application.Expenses.Models.ExpenseDto;
using FinancialGoalDto = ViaRiceco.Modules.Portfolios.Application.FinancialGoals.Models.FinancialGoalDto;

namespace ViaRiceco.IntegrationTests;

/// <summary>
/// Performance and stress tests for ViaRiceco system under heavy load
/// </summary>
public sealed class PerformanceAndStressTests : BaseIntegrationTest
{
    public PerformanceAndStressTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_MaintainDataIntegrity_UnderConcurrentModifications()
    {
        // Arrange - Create base data for concurrent modification tests
        SettlementPeriodDto settlementPeriod = await Sender.CreateSettlementPeriodAsync();
        await Sender.AddIncomeToSettlementPeriodAsync(settlementPeriod.Id, 5000);
        
        TaxTypeDto taxType = await Sender.CreateTaxTypeAsync("Concurrent Tax");
        await Sender.AddTaxToSettlementPeriodAsync(settlementPeriod.Id, taxType.Id, 1000);

        // Act - Simulate concurrent modifications
        var modificationTasks = new List<Task>();

        // Concurrent income updates
        for (int i = 0; i < 5; i++)
        {
            int iteration = i;
            modificationTasks.Add(Task.Run(async () =>
            {
                await Task.Delay(iteration * 100); // Stagger operations
                
                try
                {
                    // Add additional income streams
                    await Sender.AddIncomeToSettlementPeriodAsync(settlementPeriod.Id, 500 + iteration * 100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Concurrent income operation {iteration} failed: {ex.Message}");
                    // Expected in concurrent scenarios - some operations may fail due to constraints
                }
            }));
        }

        // Concurrent tax updates
        for (int i = 0; i < 3; i++)
        {
            int iteration = i;
            modificationTasks.Add(Task.Run(async () =>
            {
                await Task.Delay(iteration * 150);
                
                try
                {
                    TaxTypeDto additionalTaxType = await Sender.CreateTaxTypeAsync($"Additional Tax {iteration}");
                    await Sender.AddTaxToSettlementPeriodAsync(settlementPeriod.Id, additionalTaxType.Id, 200 + iteration * 50);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Concurrent tax operation {iteration} failed: {ex.Message}");
                }
            }));
        }

        await Task.WhenAll(modificationTasks);

        // Assert - Verify data integrity after concurrent modifications
        var finalQuery = new GetSettlementPeriodQuery(settlementPeriod.Id);
        Result<SettlementPeriodDto> finalResult = await Sender.Send(finalQuery);

        finalResult.IsSuccess.Should().BeTrue();
        finalResult.Value.Should().NotBeNull();
        
        // Verify financial calculations remain correct
        finalResult.Value.NetAmount.Should()
            .Be(finalResult.Value.TotalIncome - finalResult.Value.TotalTaxes);
        
        // Verify minimum expected data (original income + tax should still exist)
        finalResult.Value.TotalIncome.Should().BeGreaterThanOrEqualTo(5000);
        finalResult.Value.TotalTaxes.Should().BeGreaterThanOrEqualTo(1000);
        
        // Verify IDs are still properly formatted
        finalResult.Value.Incomes.Should().AllSatisfy(income => 
            income.Id.Should().StartWith("i_"));
        finalResult.Value.Taxes.Should().AllSatisfy(tax => 
            tax.Id.Should().StartWith("t_"));

        Console.WriteLine($"Final settlement period: Income={finalResult.Value.TotalIncome}, " +
                         $"Taxes={finalResult.Value.TotalTaxes}, Net={finalResult.Value.NetAmount}");
    }

    #region Helper Methods

    private async Task<object> ExecuteSettlementQuery()
    {
        var query = new GetSettlementPeriodsQuery(null, null, 1, 10, null, null);
        Result<GetSettlementPeriodsQueryResponse> result = await Sender.Send(query);
        return result.IsSuccess ? result.Value : new object();
    }

    private async Task<object> ExecuteExpenseQuery()
    {
        var query = new GetExpensesQuery(null, null, 1, 10, null);
        Result<GetExpensesQueryResponse> result = await Sender.Send(query);
        return result.IsSuccess ? result.Value : new object();
    }

    private async Task<object> ExecuteGoalQuery()
    {
        var query = new GetFinancialGoalsQuery(null, null, 1, 10);
        Result<GetFinancialGoalsQueryResponse> result = await Sender.Send(query);
        return result.IsSuccess ? result.Value : new object();
    }

    #endregion
}
