using FluentAssertions;
using NSubstitute;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.UpdateExpenseType;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;
using ViaRiceco.Modules.Budgets.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.UnitTests.Application.ExpenseTypes;

public sealed class UpdateExpenseTypeCommandHandlerTests : BaseTest
{
    private readonly IExpenseTypeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly UpdateExpenseTypeCommandHandler _handler;

    public UpdateExpenseTypeCommandHandlerTests()
    {
        _repository = Substitute.For<IExpenseTypeRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeProvider = Substitute.For<TimeProvider>();
        _handler = new UpdateExpenseTypeCommandHandler(_repository, _unitOfWork, _timeProvider);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenExpenseTypeNotFound()
    {
        // Arrange
        string id = "et_" + Faker.Random.Guid();
        string name = Faker.Commerce.Categories(1)[0];
        var command = new UpdateExpenseTypeCommand(id, name);

        _repository.GetAsync(id, Arg.Any<CancellationToken>()).Returns((ExpenseType?)null);

        // Act
        Result<ExpenseTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ExpenseTypeErrors.NotFound(id));

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenExpenseTypeIsSystemDefined()
    {
        // Arrange
        string id = "et_" + Faker.Random.Guid();
        string originalName = "Investment";
        string newName = "Updated Investment";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        var command = new UpdateExpenseTypeCommand(id, newName);

        var expenseType = ExpenseType.Create(originalName, createdAtUtc);
        System.Reflection.PropertyInfo? isSystemDefinedProperty = typeof(ExpenseType).GetProperty("IsSystemDefined");
        isSystemDefinedProperty?.SetValue(expenseType, true);

        _repository.GetAsync(id, Arg.Any<CancellationToken>()).Returns(expenseType);

        // Act
        Result<ExpenseTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ExpenseTypeErrors.CannotUpdateSystemDefined());

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenNameAlreadyExistsForDifferentExpenseType()
    {
        // Arrange
        string id = "et_" + Faker.Random.Guid();
        string originalName = "Travel";
        string newName = "Office Supplies";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        var command = new UpdateExpenseTypeCommand(id, newName);

        var expenseType = ExpenseType.Create(originalName, createdAtUtc);
        
        _repository.GetAsync(id, Arg.Any<CancellationToken>()).Returns(expenseType);
        _repository.ExistsByNameAsync(newName, id, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        Result<ExpenseTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ExpenseTypeErrors.DuplicateName(newName));

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenUpdateIsValid()
    {
        // Arrange
        string id = "et_" + Faker.Random.Guid();
        string originalName = "Travel";
        string newName = "Business Travel";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        var command = new UpdateExpenseTypeCommand(id, newName);

        var expenseType = ExpenseType.Create(originalName, createdAtUtc);
        // Set the ID to match what the repository should return
        System.Reflection.PropertyInfo? idProperty = typeof(ExpenseType).GetProperty("Id");
        idProperty?.SetValue(expenseType, id);
        
        _repository.GetAsync(id, Arg.Any<CancellationToken>()).Returns(expenseType);
        _repository.ExistsByNameAsync(newName, id, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(updatedAtUtc);

        // Act
        Result<ExpenseTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().StartWith("et_");
        result.Value.Name.Should().Be(newName);
        result.Value.IsSystemDefined.Should().BeFalse();

        expenseType.Name.Should().Be(newName);
        expenseType.UpdatedAtUtc.Should().Be(updatedAtUtc);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenNameIsUnchanged()
    {
        // Arrange
        string id = "et_" + Faker.Random.Guid();
        string name = "Travel";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        var command = new UpdateExpenseTypeCommand(id, name);

        var expenseType = ExpenseType.Create(name, createdAtUtc);
        // Set the ID to match what the repository should return
        System.Reflection.PropertyInfo? idProperty = typeof(ExpenseType).GetProperty("Id");
        idProperty?.SetValue(expenseType, id);
        
        _repository.GetAsync(id, Arg.Any<CancellationToken>()).Returns(expenseType);
        _repository.ExistsByNameAsync(name, id, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(updatedAtUtc);

        // Act
        Result<ExpenseTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().StartWith("et_");
        result.Value.Name.Should().Be(name);

        expenseType.Name.Should().Be(name);
        expenseType.UpdatedAtUtc.Should().BeNull(); // Should not update timestamp when name is same

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryWithCorrectParameters()
    {
        // Arrange
        string id = "et_" + Faker.Random.Guid();
        string originalName = "Marketing";
        string newName = "Digital Marketing";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        var command = new UpdateExpenseTypeCommand(id, newName);

        var expenseType = ExpenseType.Create(originalName, createdAtUtc);
        
        _repository.GetAsync(id, Arg.Any<CancellationToken>()).Returns(expenseType);
        _repository.ExistsByNameAsync(newName, id, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).GetAsync(id, Arg.Any<CancellationToken>());
        await _repository.Received(1).ExistsByNameAsync(newName, id, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Travel", "Business Travel")]
    [InlineData("Office", "Office Supplies")]
    [InlineData("Meals", "Meals & Entertainment")]
    public async Task Handle_Should_HandleDifferentNameUpdates(string originalName, string newName)
    {
        // Arrange
        string id = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        var command = new UpdateExpenseTypeCommand(id, newName);

        var expenseType = ExpenseType.Create(originalName, createdAtUtc);
        
        _repository.GetAsync(id, Arg.Any<CancellationToken>()).Returns(expenseType);
        _repository.ExistsByNameAsync(newName, id, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(updatedAtUtc);

        // Act
        Result<ExpenseTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(newName);
        expenseType.Name.Should().Be(newName);
    }

    [Fact]
    public async Task Handle_Should_PassCancellationToken()
    {
        // Arrange
        string id = "et_" + Faker.Random.Guid();
        string name = Faker.Commerce.Categories(1)[0];
        var command = new UpdateExpenseTypeCommand(id, name);
        var cancellationToken = new CancellationToken(true);

        var expenseType = ExpenseType.Create("Original", Faker.Date.PastOffset().UtcDateTime);
        _repository.GetAsync(id, Arg.Any<CancellationToken>()).Returns(expenseType);

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
        await _repository.Received().GetAsync(id, cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_HandleUnicodeCharacters()
    {
        // Arrange
        string id = "et_" + Faker.Random.Guid();
        string originalName = "Travel";
        string newName = "Frais de Transport (交通費) - نفقات النقل";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        var command = new UpdateExpenseTypeCommand(id, newName);

        var expenseType = ExpenseType.Create(originalName, createdAtUtc);
        
        _repository.GetAsync(id, Arg.Any<CancellationToken>()).Returns(expenseType);
        _repository.ExistsByNameAsync(newName, id, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(updatedAtUtc);

        // Act
        Result<ExpenseTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(newName);
        expenseType.Name.Should().Be(newName);
    }
}
