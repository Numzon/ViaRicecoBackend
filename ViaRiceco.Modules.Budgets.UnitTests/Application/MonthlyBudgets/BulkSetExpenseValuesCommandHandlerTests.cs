using FluentAssertions;
using NSubstitute;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Application.MonthlyBudgetExpenses.BulkSetExpenseValues;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;
using ViaRiceco.Modules.Budgets.Domain.Expenses;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;
using ViaRiceco.Modules.Budgets.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.UnitTests.Application.MonthlyBudgets;

public sealed class BulkSetExpenseValuesCommandHandlerTests : BaseTest
{
    private readonly IMonthlyBudgetRepository _monthlyBudgetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly BulkSetExpenseValuesCommandHandler _handler;

    public BulkSetExpenseValuesCommandHandlerTests()
    {
        _monthlyBudgetRepository = Substitute.For<IMonthlyBudgetRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeProvider = Substitute.For<TimeProvider>();
        _handler = new BulkSetExpenseValuesCommandHandler(_monthlyBudgetRepository, _unitOfWork, _timeProvider);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenMonthlyBudgetNotFound()
    {
        // Arrange
        string monthlyBudgetId = "mb_" + Faker.Random.Guid();
        var expenseUpdates = new List<ExpenseValueUpdateDto>
        {
            new("mbe_" + Faker.Random.Guid(), 100.50m)
        };
        var command = new BulkSetExpenseValuesCommand(monthlyBudgetId, expenseUpdates);

        _monthlyBudgetRepository.GetAsync(monthlyBudgetId, Arg.Any<CancellationToken>())
            .Returns((MonthlyBudget?)null);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("MonthlyBudget.NotFound");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenExpenseNotFound()
    {
        // Arrange
        string monthlyBudgetId = "mb_" + Faker.Random.Guid();
        string nonExistentExpenseId = "mbe_" + Faker.Random.Guid();
        var expenseUpdates = new List<ExpenseValueUpdateDto>
        {
            new(nonExistentExpenseId, 100.50m)
        };
        var command = new BulkSetExpenseValuesCommand(monthlyBudgetId, expenseUpdates);

        MonthlyBudget monthlyBudget = CreateMonthlyBudgetWithExpenses();
        _monthlyBudgetRepository.GetAsync(monthlyBudgetId, Arg.Any<CancellationToken>())
            .Returns(monthlyBudget);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("MonthlyBudget.ExpenseNotFound");
    }

    [Fact]
    public async Task Handle_Should_UpdateExpenseValues_WhenValidCommand()
    {
        // Arrange
        string monthlyBudgetId = "mb_" + Faker.Random.Guid();
        MonthlyBudget monthlyBudget = CreateMonthlyBudgetWithExpenses();
        MonthlyBudgetExpense expense1 = monthlyBudget.Expenses.First();
        MonthlyBudgetExpense expense2 = monthlyBudget.Expenses.Skip(1).First();
        
        var expenseUpdates = new List<ExpenseValueUpdateDto>
        {
            new(expense1.Id, 100.50m),
            new(expense2.Id, 200.75m)
        };
        var command = new BulkSetExpenseValuesCommand(monthlyBudgetId, expenseUpdates);
        
        DateTime utcNow = Faker.Date.RecentOffset().UtcDateTime;

        _monthlyBudgetRepository.GetAsync(monthlyBudgetId, Arg.Any<CancellationToken>())
            .Returns(monthlyBudget);
        _timeProvider.UtcNow().Returns(utcNow);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        expense1.Value.Should().Be(100.50m);
        expense2.Value.Should().Be(200.75m);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryWithCorrectParameters()
    {
        // Arrange
        string monthlyBudgetId = "mb_" + Faker.Random.Guid();
        MonthlyBudget monthlyBudget = CreateMonthlyBudgetWithExpenses();
        MonthlyBudgetExpense expense = monthlyBudget.Expenses.First();
        
        var expenseUpdates = new List<ExpenseValueUpdateDto>
        {
            new(expense.Id, 150.25m)
        };
        var command = new BulkSetExpenseValuesCommand(monthlyBudgetId, expenseUpdates);
        
        DateTime utcNow = new(2024, 3, 15, 10, 30, 0, DateTimeKind.Utc);

        _monthlyBudgetRepository.GetAsync(monthlyBudgetId, Arg.Any<CancellationToken>())
            .Returns(monthlyBudget);
        _timeProvider.UtcNow().Returns(utcNow);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _monthlyBudgetRepository.Received(1).GetAsync(monthlyBudgetId, Arg.Any<CancellationToken>());
        _timeProvider.Received(1).GetUtcNow();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ConvertDtosToExpenseValueUpdates()
    {
        // Arrange
        string monthlyBudgetId = "mb_" + Faker.Random.Guid();
        MonthlyBudget monthlyBudget = CreateMonthlyBudgetWithExpenses();
        MonthlyBudgetExpense expense1 = monthlyBudget.Expenses.First();
        MonthlyBudgetExpense expense2 = monthlyBudget.Expenses.Skip(1).First();
        
        var expenseUpdates = new List<ExpenseValueUpdateDto>
        {
            new(expense1.Id, 100.50m),
            new(expense2.Id, null) // Test null value
        };
        var command = new BulkSetExpenseValuesCommand(monthlyBudgetId, expenseUpdates);
        
        DateTime utcNow = Faker.Date.RecentOffset().UtcDateTime;

        _monthlyBudgetRepository.GetAsync(monthlyBudgetId, Arg.Any<CancellationToken>())
            .Returns(monthlyBudget);
        _timeProvider.UtcNow().Returns(utcNow);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        expense1.Value.Should().Be(100.50m);
        expense2.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_HandleEmptyUpdatesList()
    {
        // Arrange
        string monthlyBudgetId = "mb_" + Faker.Random.Guid();
        MonthlyBudget monthlyBudget = CreateMonthlyBudgetWithExpenses();
        var expenseUpdates = new List<ExpenseValueUpdateDto>();
        var command = new BulkSetExpenseValuesCommand(monthlyBudgetId, expenseUpdates);

        _monthlyBudgetRepository.GetAsync(monthlyBudgetId, Arg.Any<CancellationToken>())
            .Returns(monthlyBudget);
        _timeProvider.UtcNow().Returns(Faker.Date.RecentOffset().UtcDateTime);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private MonthlyBudget CreateMonthlyBudgetWithExpenses()
    {
        var expense1 = Expense.Create("Groceries", "et_" + Faker.Random.Guid(), null, Faker.Date.PastOffset().UtcDateTime);
        var expense2 = Expense.Create("Gas", "et_" + Faker.Random.Guid(), null, Faker.Date.PastOffset().UtcDateTime);
        var expense3 = Expense.Create("Rent", "et_" + Faker.Random.Guid(), null, Faker.Date.PastOffset().UtcDateTime);

        var expenseType1 = ExpenseType.Create("Food", Faker.Date.PastOffset().UtcDateTime);
        var expenseType2 = ExpenseType.Create("Transportation", Faker.Date.PastOffset().UtcDateTime);
        var expenseType3 = ExpenseType.Create("Housing", Faker.Date.PastOffset().UtcDateTime);

        var expenseData = new List<ExpenseData>
        {
            new ExpenseData(expense1, expenseType1),
            new ExpenseData(expense2, expenseType2),
            new ExpenseData(expense3, expenseType3)
        };

        return MonthlyBudget.CreateFromSettlementPeriod(
            "sp_" + Faker.Random.Guid(),
            Faker.Random.Int(1, 12),
            Faker.Random.Int(2020, 2025),
            Faker.Random.Decimal(1000, 5000),
            expenseData,
            Faker.Date.RecentOffset().UtcDateTime);
    }
}
