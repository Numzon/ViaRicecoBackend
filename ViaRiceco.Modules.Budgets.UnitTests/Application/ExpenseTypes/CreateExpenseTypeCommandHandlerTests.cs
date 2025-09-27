using FluentAssertions;
using NSubstitute;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.CreateExpenseType;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;
using ViaRiceco.Modules.Budgets.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.UnitTests.Application.ExpenseTypes;

public sealed class CreateExpenseTypeCommandHandlerTests : BaseTest
{
    private readonly IExpenseTypeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly CreateExpenseTypeCommandHandler _handler;

    public CreateExpenseTypeCommandHandlerTests()
    {
        _repository = Substitute.For<IExpenseTypeRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeProvider = Substitute.For<TimeProvider>();
        _handler = new CreateExpenseTypeCommandHandler(_repository, _unitOfWork, _timeProvider);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenExpenseTypeNameIsUnique()
    {
        // Arrange
        string name = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        var command = new CreateExpenseTypeCommand(name);

        _repository.ExistsByNameAsync(name, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(createdAtUtc);

        // Act
        Result<ExpenseTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Id.Should().StartWith("et_");
        result.Value.IsSystemDefined.Should().BeFalse();

        _repository.Received(1).Insert(Arg.Is<ExpenseType>(et => et.Name == name && !et.IsSystemDefined));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenExpenseTypeNameExists()
    {
        // Arrange
        string name = Faker.Commerce.Categories(1)[0];
        var command = new CreateExpenseTypeCommand(name);

        _repository.ExistsByNameAsync(name, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        Result<ExpenseTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ExpenseTypeErrors.DuplicateName(name));

        _repository.DidNotReceive().Insert(Arg.Any<ExpenseType>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryWithCorrectParameters()
    {
        // Arrange
        string name = "Travel Expenses";
        DateTime createdAtUtc = new(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var command = new CreateExpenseTypeCommand(name);

        _repository.ExistsByNameAsync(name, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(createdAtUtc);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).ExistsByNameAsync(name, Arg.Any<CancellationToken>());
        _repository.Received(1).Insert(Arg.Is<ExpenseType>(et => 
            et.Name == name && 
            et.CreatedAtUtc == createdAtUtc &&
            !et.IsSystemDefined));
    }

    [Theory]
    [InlineData("Travel")]
    [InlineData("Office Supplies")]
    [InlineData("Meals & Entertainment")]
    public async Task Handle_Should_HandleDifferentExpenseTypeNames(string name)
    {
        // Arrange
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        var command = new CreateExpenseTypeCommand(name);

        _repository.ExistsByNameAsync(name, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(createdAtUtc);

        // Act
        Result<ExpenseTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
    }

    [Fact]
    public async Task Handle_Should_PassCancellationToken()
    {
        // Arrange
        string name = Faker.Commerce.Categories(1)[0];
        var command = new CreateExpenseTypeCommand(name);
        var cancellationToken = new CancellationToken(true);

        _repository.ExistsByNameAsync(name, Arg.Any<CancellationToken>()).Returns(false);

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
        await _repository.Received().ExistsByNameAsync(name, cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_ReturnExpenseTypeDtoWithCorrectProperties()
    {
        // Arrange
        string name = "Marketing Expenses";
        DateTime createdAtUtc = new(2024, 3, 20, 14, 45, 30, DateTimeKind.Utc);
        var command = new CreateExpenseTypeCommand(name);

        _repository.ExistsByNameAsync(name, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(createdAtUtc);

        // Act
        Result<ExpenseTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(name);
        result.Value.Id.Should().StartWith("et_");
        result.Value.Id.Should().HaveLength(39); // "et_" + GUID length
        result.Value.IsSystemDefined.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_HandleEmptyName()
    {
        // Arrange
        string name = string.Empty;
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        var command = new CreateExpenseTypeCommand(name);

        _repository.ExistsByNameAsync(name, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(createdAtUtc);

        // Act
        Result<ExpenseTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
    }

    [Fact]
    public async Task Handle_Should_HandleWhitespaceOnlyName()
    {
        // Arrange
        string name = "   ";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        var command = new CreateExpenseTypeCommand(name);

        _repository.ExistsByNameAsync(name, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(createdAtUtc);

        // Act
        Result<ExpenseTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
    }
}
