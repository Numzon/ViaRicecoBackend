using FluentAssertions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.CreateExpenseType;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;
using ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.ExpenseTypes;

public sealed class CreateExpenseTypeTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_CreateExpenseType_WhenValidDataProvided()
    {
        // Arrange
        await CleanDatabaseAsync();
        var command = new CreateExpenseTypeCommand("Business Expenses");

        // Act
        Result<ExpenseTypeDto> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Business Expenses");
        result.Value.IsSystemDefined.Should().BeFalse();
        result.Value.Id.Should().StartWith("et_");
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenExpenseTypeNameAlreadyExists()
    {
        // Arrange
        await CleanDatabaseAsync();
        var command1 = new CreateExpenseTypeCommand("Duplicate Name");
        var command2 = new CreateExpenseTypeCommand("Duplicate Name");

        // Act
        await Sender.Send(command1);
        Result<ExpenseTypeDto> result = await Sender.Send(command2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ExpenseTypeErrors.DuplicateName("Duplicate Name").Code);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenExpenseTypeNameIsEmpty()
    {
        // Arrange
        await CleanDatabaseAsync();
        var command = new CreateExpenseTypeCommand("");

        // Act
        Result<ExpenseTypeDto> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenExpenseTypeNameIsTooLong()
    {
        // Arrange
        await CleanDatabaseAsync();
        string longName = new('A', 201); // Max length is 200
        var command = new CreateExpenseTypeCommand(longName);

        // Act
        Result<ExpenseTypeDto> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenTryingToCreateSystemDefinedExpenseType()
    {
        // Arrange
        await CleanDatabaseAsync();
        var command = new CreateExpenseTypeCommand("Investment"); // This conflicts with seeded system expense type

        // Act
        Result<ExpenseTypeDto> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ExpenseTypeErrors.DuplicateName("Investment").Code);
    }
}