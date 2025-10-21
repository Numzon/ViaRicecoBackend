using Bogus;
using FluentAssertions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.Incomes.AddIncome;
using ViaRiceco.Modules.Accounting.Application.Taxes.AddTax;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.CreateSettlementPeriod;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.CreateTaxType;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Budgets.Application.Expenses.CreateExpense;
using ViaRiceco.Modules.Budgets.Application.Expenses.Models;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.CreateExpenseType;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Portfolios.Application.Currencies.CreateCurrency;
using ViaRiceco.Modules.Portfolios.Application.Currencies.Models;
using ViaRiceco.Modules.Portfolios.Application.FinancialGoals.CreateFinancialGoal;
using ViaRiceco.Modules.Portfolios.Application.FinancialGoals.Models;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.CreateInvestmentStrategy;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.Models;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategyTypes.CreateInvestmentStrategyType;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategyTypes.Models;
using MediatR;
using SettlementPeriodDto = ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models.SettlementPeriodDto;
using IncomeDto = ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models.IncomeDto;
using TaxDto = ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models.TaxDto;
using ExpenseDto = ViaRiceco.Modules.Budgets.Application.Expenses.Models.ExpenseDto;
using ExpenseTypeDto = ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models.ExpenseTypeDto;
using TaxTypeDto = ViaRiceco.Modules.Accounting.Application.TaxTypes.Models.TaxTypeDto;
using FinancialGoalDto = ViaRiceco.Modules.Portfolios.Application.FinancialGoals.Models.FinancialGoalDto;
using CurrencyDto = ViaRiceco.Modules.Portfolios.Application.Currencies.Models.CurrencyDto;
using InvestmentStrategyDto = ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.Models.InvestmentStrategyDto;
using InvestmentStrategyTypeDto = ViaRiceco.Modules.Portfolios.Application.InvestmentStrategyTypes.Models.InvestmentStrategyTypeDto;

namespace ViaRiceco.IntegrationTests.Abstractions;

/// <summary>
/// Provides helper methods for creating test data across all ViaRiceco modules
/// </summary>
internal static class CommandHelpers
{
    private static readonly Faker Faker = new();

    #region Accounting Module Helpers

    /// <summary>
    /// Creates a settlement period with the specified month and year
    /// </summary>
    internal static async Task<SettlementPeriodDto> CreateSettlementPeriodAsync(
        this ISender sender,
        int month = 0,
        int year = 0)
    {
        month = month == 0 ? Faker.Random.Int(1, 12) : month;
        year = year == 0 ? Faker.Random.Int(2020, 2030) : year;

        var command = new CreateSettlementPeriodCommand(month, year);
        Result<SettlementPeriodDto> result = await sender.Send(command);

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    /// <summary>
    /// Creates a tax type with a random or specified name
    /// </summary>
    internal static async Task<TaxTypeDto> CreateTaxTypeAsync(
        this ISender sender,
        string? name = null)
    {
        name ??= $"{Faker.Finance.AccountName()}_{Guid.NewGuid()} Tax";

        var command = new CreateTaxTypeCommand(name);
        Result<TaxTypeDto> result = await sender.Send(command);

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    /// <summary>
    /// Adds income to a settlement period
    /// </summary>
    internal static async Task<IncomeDto> AddIncomeToSettlementPeriodAsync(
        this ISender sender,
        string settlementPeriodId,
        decimal amount = 0)
    {
        amount = amount == 0 ? Faker.Random.Decimal(1000, 10000) : amount;

        var command = new AddIncomeCommand(settlementPeriodId, amount);
        Result<IncomeDto> result = await sender.Send(command);

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    /// <summary>
    /// Adds tax to a settlement period
    /// </summary>
    internal static async Task<TaxDto> AddTaxToSettlementPeriodAsync(
        this ISender sender,
        string settlementPeriodId,
        string taxTypeId,
        decimal amount = 0)
    {
        amount = amount == 0 ? Faker.Random.Decimal(100, 1000) : amount;

        var command = new AddTaxCommand(settlementPeriodId, amount, taxTypeId);
        Result<TaxDto> result = await sender.Send(command);

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    #endregion

    #region Budgets Module Helpers

    /// <summary>
    /// Creates an expense type with a random or specified name
    /// </summary>
    internal static async Task<ExpenseTypeDto> CreateExpenseTypeAsync(
        this ISender sender,
        string? name = null)
    {
        name ??= Faker.Commerce.Categories(1)[0];

        var command = new CreateExpenseTypeCommand(name);
        Result<ExpenseTypeDto> result = await sender.Send(command);

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    /// <summary>
    /// Creates an expense with the specified or random data
    /// </summary>
    internal static async Task<ExpenseDto> CreateExpenseAsync(
        this ISender sender,
        string? expenseTypeId = null,
        string? name = null)
    {
        if (expenseTypeId is null)
        {
            ExpenseTypeDto expenseType = await sender.CreateExpenseTypeAsync();
            expenseTypeId = expenseType.Id;
        }

        name ??= Faker.Commerce.ProductName();

        var command = new CreateExpenseCommand(name, expenseTypeId, null);
        Result<ExpenseDto> result = await sender.Send(command);

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    /// <summary>
    /// Creates multiple expenses across different expense types
    /// </summary>
    internal static async Task<List<ExpenseDto>> CreateMultipleExpensesAsync(
        this ISender sender,
        int count = 3)
    {
        var expenses = new List<ExpenseDto>();

        for (int i = 0; i < count; i++)
        {
            ExpenseDto expense = await sender.CreateExpenseAsync();
            expenses.Add(expense);
        }

        return expenses;
    }

    #endregion

    #region Portfolios Module Helpers

    /// <summary>
    /// Creates a currency with the specified or random data
    /// </summary>
    internal static async Task<CurrencyDto> CreateCurrencyAsync(
        this ISender sender,
        string? code = null,
        string? name = null)
    {
        code ??= Faker.Finance.Currency().Code;
        name ??= Faker.Finance.Currency().Description;

        var command = new CreateCurrencyCommand(code, name);
        Result<CurrencyDto> result = await sender.Send(command);

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    /// <summary>
    /// Creates an investment strategy type
    /// </summary>
    internal static async Task<InvestmentStrategyTypeDto> CreateInvestmentStrategyTypeAsync(
        this ISender sender,
        string? name = null)
    {
        name ??= $"{Faker.Finance.AccountName()} Strategy";

        var command = new CreateInvestmentStrategyTypeCommand(name);
        Result<InvestmentStrategyTypeDto> result = await sender.Send(command);

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    /// <summary>
    /// Creates a financial goal with specified or random data
    /// </summary>
    internal static async Task<FinancialGoalDto> CreateFinancialGoalAsync(
        this ISender sender,
        string? name = null,
        string? parentId = null)
    {
        name ??= $"Goal: {Faker.Lorem.Words(2).Aggregate((a, b) => $"{a} {b}")}"; 

        var command = new CreateFinancialGoalCommand(name, parentId);
        Result<FinancialGoalDto> result = await sender.Send(command);

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    /// <summary>
    /// Creates an investment strategy with specified or random data
    /// </summary>
    internal static async Task<InvestmentStrategyDto> CreateInvestmentStrategyAsync(
        this ISender sender,
        string? investmentStrategyTypeId = null,
        string? name = null,
        decimal uninvestedAmount = 0)
    {
        if (investmentStrategyTypeId is null)
        {
            InvestmentStrategyTypeDto strategyType = await sender.CreateInvestmentStrategyTypeAsync();
            investmentStrategyTypeId = strategyType.Id;
        }

        name ??= $"{Faker.Finance.AccountName()} Investment Strategy";
        uninvestedAmount = uninvestedAmount == 0 ? Faker.Random.Decimal(1000, 50000) : uninvestedAmount;

        var command = new CreateInvestmentStrategyCommand(investmentStrategyTypeId, name, uninvestedAmount);
        Result<InvestmentStrategyDto> result = await sender.Send(command);

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    /// <summary>
    /// Creates a complete portfolio scenario with multiple entities
    /// </summary>
    internal static async Task<(CurrencyDto currency, InvestmentStrategyDto strategy, FinancialGoalDto goal)>
        CreateCompletePortfolioAsync(this ISender sender)
    {
        CurrencyDto currency = await sender.CreateCurrencyAsync();
        InvestmentStrategyDto strategy = await sender.CreateInvestmentStrategyAsync();
        FinancialGoalDto goal = await sender.CreateFinancialGoalAsync();

        return (currency, strategy, goal);
    }

    #endregion

}

/// <summary>
/// Represents a complete financial scenario for testing cross-module functionality
/// </summary>
internal sealed class CompleteFinancialScenario
{
    public List<(SettlementPeriodDto settlementPeriod, IncomeDto income, TaxDto tax)> SettlementPeriods { get; set; } = [];
    public List<ExpenseDto> Expenses { get; set; } = [];
    public CurrencyDto Currency { get; set; } = null!;
    public InvestmentStrategyDto InvestmentStrategy { get; set; } = null!;
    public FinancialGoalDto FinancialGoal { get; set; } = null!;
}
