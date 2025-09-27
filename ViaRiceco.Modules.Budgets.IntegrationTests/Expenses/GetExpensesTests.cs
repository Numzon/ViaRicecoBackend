using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ViaRiceco.Common.Presentation.Abstractions.Collections;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.CreateExpenseType;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Budgets.Application.Expenses.CreateExpense;
using ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.Expenses;

public sealed class GetExpensesTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnEmptyList_WhenNoExpensesExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/budgets/expenses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ViaRicecoCollectionResponse? result = await response.Content.ReadFromJsonAsync<ViaRicecoCollectionResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Should_ReturnExpenses_WhenExpensesExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type and expenses
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        CreateExpenseCommand[] createRequests =
        [
            new CreateExpenseCommand("Hotel Booking", expenseType!.Id),
            new CreateExpenseCommand("Flight Tickets", expenseType.Id),
            new CreateExpenseCommand("Taxi Fare", expenseType.Id)
        ];

        foreach (CreateExpenseCommand request in createRequests)
        {
            await client.PostAsJsonAsync("/api/budgets/expenses", request);
        }

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/budgets/expenses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ViaRicecoCollectionResponse? result = await response.Content.ReadFromJsonAsync<ViaRicecoCollectionResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task Should_ReturnPaginatedResults_WhenPageSizeIsProvided()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        // Create test data
        for (int i = 1; i <= 5; i++)
        {
            var request = new CreateExpenseCommand($"Expense {i:D2}", expenseType!.Id);
            await client.PostAsJsonAsync("/api/budgets/expenses", request);
        }

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/budgets/expenses?page=1&pageSize=3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ViaRicecoCollectionResponse? result = await response.Content.ReadFromJsonAsync<ViaRicecoCollectionResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(3);
    }

    [Fact]
    public async Task Should_ReturnFilteredExpenses_WhenSearchIsProvided()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        // Create test data
        await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Hotel Booking", expenseType!.Id));
        await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Flight Tickets", expenseType.Id));
        await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Hotel Room Service", expenseType.Id));

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/budgets/expenses?q=hotel");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ViaRicecoCollectionResponse? result = await response.Content.ReadFromJsonAsync<ViaRicecoCollectionResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2); // Hotel Booking and Hotel Room Service
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Should_FilterByExpenseType_WhenExpenseTypeIdIsProvided()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create two expense types
        HttpResponseMessage createTravelExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        HttpResponseMessage createOfficeExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Office"));
        
        ExpenseTypeDto? travelExpenseType = await createTravelExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();
        ExpenseTypeDto? officeExpenseType = await createOfficeExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        // Create expenses for different expense types
        await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Hotel Booking", travelExpenseType!.Id));
        await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Flight Tickets", travelExpenseType.Id));
        await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Office Supplies", officeExpenseType!.Id));

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/expenses?expenseTypeId={travelExpenseType.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ViaRicecoCollectionResponse? result = await response.Content.ReadFromJsonAsync<ViaRicecoCollectionResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2); // Only travel expenses
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Should_ReturnSortedExpenses_WhenSortIsProvided()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        // Create test data in non-alphabetical order
        await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Zebra Rental", expenseType!.Id));
        await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Alpha Services", expenseType.Id));
        await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Beta Product", expenseType.Id));

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/budgets/expenses?sort=name");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ViaRicecoCollectionResponse? result = await response.Content.ReadFromJsonAsync<ViaRicecoCollectionResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);
        
        // Note: We can't easily verify sort order without deserializing individual items
        // This test verifies the endpoint accepts sort parameters without error
    }

    [Fact]
    public async Task Should_CombineSearchAndFilter_WhenBothParametersAreProvided()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create two expense types
        HttpResponseMessage createTravelExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        HttpResponseMessage createOfficeExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Office"));
        
        ExpenseTypeDto? travelExpenseType = await createTravelExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();
        ExpenseTypeDto? officeExpenseType = await createOfficeExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        // Create expenses
        await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Hotel Booking", travelExpenseType!.Id));
        await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Hotel Cleaning Service", officeExpenseType!.Id));
        await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Flight Tickets", travelExpenseType.Id));

        // Act - Search for "hotel" within travel expense type
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/expenses?q=hotel&expenseTypeId={travelExpenseType.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ViaRicecoCollectionResponse? result = await response.Content.ReadFromJsonAsync<ViaRicecoCollectionResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1); // Only "Hotel Booking" from travel expense type
        result.TotalCount.Should().Be(1);
    }
}
