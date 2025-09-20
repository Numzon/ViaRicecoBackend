using FluentAssertions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.CreateExpenseType;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.GetExpenseTypes;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.ExpenseTypes;

public sealed class GetExpenseTypesTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnSystemDefinedExpenseType_WhenNoUserExpenseTypesExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        var query = new GetExpenseTypesQuery(null, "id", 1, 10);

        // Act
        Result<GetExpenseTypesQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items.First().Name.Should().Be("Investment");
        result.Value.Items.First().IsSystemDefined.Should().BeTrue();
    }

    [Fact]
    public async Task Should_ReturnExpenseTypes_WhenExpenseTypesExist()
    {
        // Arrange
        await CleanDatabaseAsync();

        var createCommand1 = new CreateExpenseTypeCommand("Personal Expenses");
        var createCommand2 = new CreateExpenseTypeCommand("Business Expenses");
        var createCommand3 = new CreateExpenseTypeCommand("Travel Expenses");

        await Sender.Send(createCommand1);
        await Sender.Send(createCommand2);
        await Sender.Send(createCommand3);

        var query = new GetExpenseTypesQuery(null, "id", 1, 10);

        // Act
        Result<GetExpenseTypesQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(4); // 3 user-created + 1 seeded
        result.Value.TotalCount.Should().Be(4);
    }

    [Fact]
    public async Task Should_FilterExpenseTypesByName_WhenSearchProvided()
    {
        // Arrange
        await CleanDatabaseAsync();

        var createCommand1 = new CreateExpenseTypeCommand("Personal Expenses");
        var createCommand2 = new CreateExpenseTypeCommand("Business Expenses");
        var createCommand3 = new CreateExpenseTypeCommand("Travel Expenses");

        await Sender.Send(createCommand1);
        await Sender.Send(createCommand2);
        await Sender.Send(createCommand3);

        var query = new GetExpenseTypesQuery("Personal", "id", 1, 10);

        // Act
        Result<GetExpenseTypesQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items.First().Name.Should().Contain("Personal");
    }

    [Fact]
    public async Task Should_SortExpenseTypes_WhenOrderByProvided()
    {
        // Arrange
        await CleanDatabaseAsync();

        var createCommand1 = new CreateExpenseTypeCommand("Zebra Expenses");
        var createCommand2 = new CreateExpenseTypeCommand("Apple Expenses");
        var createCommand3 = new CreateExpenseTypeCommand("Business Expenses");

        await Sender.Send(createCommand1);
        await Sender.Send(createCommand2);
        await Sender.Send(createCommand3);

        var query = new GetExpenseTypesQuery(null, "name", 1, 10);

        // Act
        Result<GetExpenseTypesQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Where(x => !x.IsSystemDefined).Should().HaveCount(3);
        result.Value.Items.First().Name.Should().Be("Apple Expenses");
        result.Value.Items.Last().Name.Should().Be("Zebra Expenses");
    }

    [Fact]
    public async Task Should_ReturnPaginatedResults_WhenPageSizeProvided()
    {
        // Arrange
        await CleanDatabaseAsync();

        for (int i = 1; i <= 5; i++)
        {
            var createCommand = new CreateExpenseTypeCommand($"Expense Type {i}");
            await Sender.Send(createCommand);
        }

        var query = new GetExpenseTypesQuery(null, "id", 1, 3);

        // Act
        Result<GetExpenseTypesQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
        result.Value.TotalCount.Should().Be(6); // 5 user-created + 1 seeded
    }
}
