using FluentAssertions;
using NSubstitute;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Application.Expenses.CreateExpense;
using ViaRiceco.Modules.Budgets.Application.Expenses.Models;
using ViaRiceco.Modules.Budgets.Domain.Expenses;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;
using ViaRiceco.Modules.Budgets.Domain.Banks;
using ViaRiceco.Modules.Budgets.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.UnitTests.Application.Expenses;

public sealed class CreateExpenseCommandHandlerTests : BaseTest
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseTypeRepository _expenseTypeRepository;
    private readonly IBankRepository _bankRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly CreateExpenseCommandHandler _handler;

    public CreateExpenseCommandHandlerTests()
    {
        _expenseRepository = Substitute.For<IExpenseRepository>();
        _expenseTypeRepository = Substitute.For<IExpenseTypeRepository>();
        _bankRepository = Substitute.For<IBankRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeProvider = Substitute.For<TimeProvider>();
        _handler = new CreateExpenseCommandHandler(_expenseRepository, _expenseTypeRepository, _bankRepository, _unitOfWork, _timeProvider);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenExpenseTypeNotFound()
    {
        // Arrange
        string name = Faker.Commerce.ProductName();
        string expenseTypeId = "et_" + Faker.Random.Guid();
        var command = new CreateExpenseCommand(name, expenseTypeId, null);

        _expenseTypeRepository.GetAsync(expenseTypeId, Arg.Any<CancellationToken>()).Returns((ExpenseType?)null);

        // Act
        Result<ExpenseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ExpenseTypeErrors.NotFound(expenseTypeId));

        _expenseRepository.DidNotReceive().Insert(Arg.Any<Expense>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenExpenseNameAlreadyExistsInExpenseType()
    {
        // Arrange
        string name = "Hotel Accommodation";
        string expenseTypeId = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        var command = new CreateExpenseCommand(name, expenseTypeId, null);

        var expenseType = ExpenseType.Create("Travel", createdAtUtc);
        
        _expenseTypeRepository.GetAsync(expenseTypeId, Arg.Any<CancellationToken>()).Returns(expenseType);
        _expenseRepository.ExistsByNameAndExpenseTypeAsync(name, expenseTypeId, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        Result<ExpenseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ExpenseErrors.DuplicateNameInExpenseType(name, expenseTypeId));

        _expenseRepository.DidNotReceive().Insert(Arg.Any<Expense>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenExpenseIsValid()
    {
        // Arrange
        string name = "Hotel Accommodation";
        string expenseTypeId = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        var command = new CreateExpenseCommand(name, expenseTypeId, null);

        var expenseType = ExpenseType.Create("Travel", Faker.Date.PastOffset().UtcDateTime);
        
        _expenseTypeRepository.GetAsync(expenseTypeId, Arg.Any<CancellationToken>()).Returns(expenseType);
        _expenseRepository.ExistsByNameAndExpenseTypeAsync(name, expenseTypeId, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(createdAtUtc);

        // Act
        Result<ExpenseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.ExpenseTypeId.Should().Be(expenseTypeId);
        result.Value.Id.Should().StartWith("e_");

        _expenseRepository.Received(1).Insert(Arg.Is<Expense>(e => 
            e.Name == name && 
            e.ExpenseTypeId == expenseTypeId &&
            e.CreatedAtUtc == createdAtUtc));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_CallRepositoriesWithCorrectParameters()
    {
        // Arrange
        string name = "Flight Tickets";
        string expenseTypeId = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = new(2024, 2, 15, 9, 30, 0, DateTimeKind.Utc);
        var command = new CreateExpenseCommand(name, expenseTypeId, null);

        var expenseType = ExpenseType.Create("Travel", Faker.Date.PastOffset().UtcDateTime);
        
        _expenseTypeRepository.GetAsync(expenseTypeId, Arg.Any<CancellationToken>()).Returns(expenseType);
        _expenseRepository.ExistsByNameAndExpenseTypeAsync(name, expenseTypeId, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(createdAtUtc);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _expenseTypeRepository.Received(1).GetAsync(expenseTypeId, Arg.Any<CancellationToken>());
        await _expenseRepository.Received(1).ExistsByNameAndExpenseTypeAsync(name, expenseTypeId, Arg.Any<CancellationToken>());
        _expenseRepository.Received(1).Insert(Arg.Is<Expense>(e => 
            e.Name == name && 
            e.ExpenseTypeId == expenseTypeId &&
            e.CreatedAtUtc == createdAtUtc));
    }

    [Theory]
    [InlineData("Hotel Accommodation")]
    [InlineData("Flight Tickets")]
    [InlineData("Business Lunch")]
    [InlineData("Conference Registration")]
    public async Task Handle_Should_HandleDifferentExpenseNames(string name)
    {
        // Arrange
        string expenseTypeId = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        var command = new CreateExpenseCommand(name, expenseTypeId, null);

        var expenseType = ExpenseType.Create("Travel", Faker.Date.PastOffset().UtcDateTime);
        
        _expenseTypeRepository.GetAsync(expenseTypeId, Arg.Any<CancellationToken>()).Returns(expenseType);
        _expenseRepository.ExistsByNameAndExpenseTypeAsync(name, expenseTypeId, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(createdAtUtc);

        // Act
        Result<ExpenseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
    }

    [Fact]
    public async Task Handle_Should_AllowSameNameInDifferentExpenseTypes()
    {
        // Arrange
        string name = "Conference Registration";
        string expenseTypeId1 = "et_" + Faker.Random.Guid();
        string expenseTypeId2 = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var command1 = new CreateExpenseCommand(name, expenseTypeId1, null);
        var command2 = new CreateExpenseCommand(name, expenseTypeId2, null);

        var expenseType1 = ExpenseType.Create("Travel", Faker.Date.PastOffset().UtcDateTime);
        var expenseType2 = ExpenseType.Create("Education", Faker.Date.PastOffset().UtcDateTime);
        
        _expenseTypeRepository.GetAsync(expenseTypeId1, Arg.Any<CancellationToken>()).Returns(expenseType1);
        _expenseTypeRepository.GetAsync(expenseTypeId2, Arg.Any<CancellationToken>()).Returns(expenseType2);
        _expenseRepository.ExistsByNameAndExpenseTypeAsync(name, expenseTypeId1, Arg.Any<CancellationToken>()).Returns(false);
        _expenseRepository.ExistsByNameAndExpenseTypeAsync(name, expenseTypeId2, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(createdAtUtc);

        // Act
        Result<ExpenseDto> result1 = await _handler.Handle(command1, CancellationToken.None);
        Result<ExpenseDto> result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.ExpenseTypeId.Should().Be(expenseTypeId1);
        result2.Value.ExpenseTypeId.Should().Be(expenseTypeId2);
        result1.Value.Name.Should().Be(name);
        result2.Value.Name.Should().Be(name);
    }

    [Fact]
    public async Task Handle_Should_PassCancellationToken()
    {
        // Arrange
        string name = Faker.Commerce.ProductName();
        string expenseTypeId = "et_" + Faker.Random.Guid();
        var command = new CreateExpenseCommand(name, expenseTypeId, null);
        var cancellationToken = new CancellationToken(true);

        var expenseType = ExpenseType.Create("Travel", Faker.Date.PastOffset().UtcDateTime);
        _expenseTypeRepository.GetAsync(expenseTypeId, Arg.Any<CancellationToken>()).Returns(expenseType);

        // Act
        try
        {
            await _handler.Handle(command, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation token is cancelled
        }

        // Assert
        await _expenseTypeRepository.Received().GetAsync(expenseTypeId, cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_ReturnExpenseDtoWithCorrectProperties()
    {
        // Arrange
        string name = "Software License";
        string expenseTypeId = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = new(2024, 4, 10, 11, 15, 45, DateTimeKind.Utc);
        var command = new CreateExpenseCommand(name, expenseTypeId, null);

        var expenseType = ExpenseType.Create("Technology", Faker.Date.PastOffset().UtcDateTime);
        
        _expenseTypeRepository.GetAsync(expenseTypeId, Arg.Any<CancellationToken>()).Returns(expenseType);
        _expenseRepository.ExistsByNameAndExpenseTypeAsync(name, expenseTypeId, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(createdAtUtc);

        // Act
        Result<ExpenseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(name);
        result.Value.ExpenseTypeId.Should().Be(expenseTypeId);
        result.Value.Id.Should().StartWith("e_");
        result.Value.Id.Should().HaveLength(38); // "e_" + GUID length
    }

    [Fact]
    public async Task Handle_Should_HandleUnicodeCharacters()
    {
        // Arrange
        string name = "Hôtel à Paris (パリのホテル) - فندق في باريس";
        string expenseTypeId = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        var command = new CreateExpenseCommand(name, expenseTypeId, null);

        var expenseType = ExpenseType.Create("Travel", Faker.Date.PastOffset().UtcDateTime);
        
        _expenseTypeRepository.GetAsync(expenseTypeId, Arg.Any<CancellationToken>()).Returns(expenseType);
        _expenseRepository.ExistsByNameAndExpenseTypeAsync(name, expenseTypeId, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(createdAtUtc);

        // Act
        Result<ExpenseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
    }

    [Fact]
    public async Task Handle_Should_HandleEmptyName()
    {
        // Arrange
        string name = string.Empty;
        string expenseTypeId = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        var command = new CreateExpenseCommand(name, expenseTypeId, null);

        var expenseType = ExpenseType.Create("Travel", Faker.Date.PastOffset().UtcDateTime);
        
        _expenseTypeRepository.GetAsync(expenseTypeId, Arg.Any<CancellationToken>()).Returns(expenseType);
        _expenseRepository.ExistsByNameAndExpenseTypeAsync(name, expenseTypeId, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(createdAtUtc);

        // Act
        Result<ExpenseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
    }
}
