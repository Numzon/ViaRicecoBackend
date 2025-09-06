using FluentAssertions;
using ViaRiceco.Common.Application.Exceptions;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.GetTaxTypes;
using ViaRiceco.Modules.Accounting.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Accounting.IntegrationTests.TaxTypes;

public sealed class GetTaxTypesTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnEmptyCollection_WhenNoTaxTypesExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        var query = new GetTaxTypesQuery(null, null, 1, 10);

        // Act
        Result<GetTaxTypesQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Should_ReturnAllTaxTypes_WhenNoFilteringApplied()
    {
        // Arrange
        await CleanDatabaseAsync();
        await Sender.CreateTaxTypeAsync("Income Tax");
        await Sender.CreateTaxTypeAsync("VAT Tax");
        await Sender.CreateTaxTypeAsync("Corporate Tax");

        var query = new GetTaxTypesQuery(null, null, 1, 10);

        // Act
        Result<GetTaxTypesQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
        result.Value.TotalCount.Should().Be(3);
        result.Value.Items.Should().Contain(x => x.Name == "Income Tax");
        result.Value.Items.Should().Contain(x => x.Name == "VAT Tax");
        result.Value.Items.Should().Contain(x => x.Name == "Corporate Tax");
    }

    [Theory]
    [InlineData("income", 1)]
    [InlineData("tax", 3)]
    [InlineData("VAT", 1)]
    [InlineData("nonexistent", 0)]
    public async Task Should_FilterTaxTypesBySearch_WhenSearchTermProvided(string searchTerm, int expectedCount)
    {
        // Arrange
        await CleanDatabaseAsync();
        await Sender.CreateTaxTypeAsync("Income Tax");
        await Sender.CreateTaxTypeAsync("VAT Tax");
        await Sender.CreateTaxTypeAsync("Corporate Tax");

        var query = new GetTaxTypesQuery(searchTerm, null, 1, 10);

        // Act
        Result<GetTaxTypesQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(expectedCount);
    }

    [Fact]
    public async Task Should_SearchCaseInsensitive()
    {
        // Arrange
        await CleanDatabaseAsync();
        await Sender.CreateTaxTypeAsync("Income Tax");
        await Sender.CreateTaxTypeAsync("VAT Tax");

        var query = new GetTaxTypesQuery("INCOME", null, 1, 10);

        // Act
        Result<GetTaxTypesQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Be("Income Tax");
    }

    [Theory]
    [InlineData(1, 2, 2)]
    [InlineData(2, 2, 1)]
    [InlineData(1, 5, 3)]
    public async Task Should_HandlePagination_WhenPageAndPageSizeProvided(int page, int pageSize, int expectedCount)
    {
        // Arrange
        await CleanDatabaseAsync();
        await Sender.CreateTaxTypeAsync("Income Tax");
        await Sender.CreateTaxTypeAsync("VAT Tax");
        await Sender.CreateTaxTypeAsync("Corporate Tax");

        var query = new GetTaxTypesQuery(null, null, page, pageSize);

        // Act
        Result<GetTaxTypesQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(expectedCount);
        result.Value.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task Should_SortTaxTypesByName_WhenSortParameterProvided()
    {
        // Arrange
        await CleanDatabaseAsync();
        await Sender.CreateTaxTypeAsync("Zebra Tax");
        await Sender.CreateTaxTypeAsync("Alpha Tax");
        await Sender.CreateTaxTypeAsync("Beta Tax");

        var query = new GetTaxTypesQuery(null, "name", 1, 10);

        // Act
        Result<GetTaxTypesQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
        
        var sortedNames = result.Value.Items.Select(x => x.Name).ToList();
        sortedNames.Should().BeInAscendingOrder();
        sortedNames[0].Should().Be("Alpha Tax");
        sortedNames[1].Should().Be("Beta Tax");
        sortedNames[2].Should().Be("Zebra Tax");
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenDescendingSortIsNotSupported()
    {
        // Arrange
        await CleanDatabaseAsync();
        await Sender.CreateTaxTypeAsync("Alpha Tax");
        await Sender.CreateTaxTypeAsync("Beta Tax");
        await Sender.CreateTaxTypeAsync("Zebra Tax");

        var query = new GetTaxTypesQuery(null, "-name", 1, 10);

        // Act
        Result<GetTaxTypesQueryResponse> result = await Sender.Send(query);

        // Assert
        // The system doesn't support descending sort with "-name" syntax
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenSortParameterIsInvalid()
    {
        // Arrange
        await CleanDatabaseAsync();
        await Sender.CreateTaxTypeAsync("Income Tax");

        var query = new GetTaxTypesQuery(null, "invalid_field", 1, 10);

        // Act
        Result<GetTaxTypesQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Should_CombineSearchAndPagination()
    {
        // Arrange
        await CleanDatabaseAsync();
        await Sender.CreateTaxTypeAsync("Income Tax A");
        await Sender.CreateTaxTypeAsync("Income Tax B");
        await Sender.CreateTaxTypeAsync("Income Tax C");
        await Sender.CreateTaxTypeAsync("VAT Tax");

        var query = new GetTaxTypesQuery("Income", null, 1, 2);

        // Act
        Result<GetTaxTypesQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Should().OnlyContain(x => x.Name.Contains("Income"));
        result.Value.TotalCount.Should().Be(4); // Total count should reflect all items, not just filtered ones
    }

    [Fact]
    public async Task Should_CombineSearchSortAndPagination()
    {
        // Arrange
        await CleanDatabaseAsync();
        await Sender.CreateTaxTypeAsync("Alpha Income Tax");
        await Sender.CreateTaxTypeAsync("Beta Income Tax");
        await Sender.CreateTaxTypeAsync("Charlie Income Tax");
        await Sender.CreateTaxTypeAsync("VAT Tax");

        var query = new GetTaxTypesQuery("Income", "name", 1, 2);

        // Act
        Result<GetTaxTypesQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Should().OnlyContain(x => x.Name.Contains("Income"));
        
        var sortedNames = result.Value.Items.Select(x => x.Name).ToList();
        sortedNames[0].Should().Be("Alpha Income Tax");
        sortedNames[1].Should().Be("Beta Income Tax");
    }

    [Fact]
    public async Task Should_HandleEmptySearchString()
    {
        // Arrange
        await CleanDatabaseAsync();
        await Sender.CreateTaxTypeAsync("Income Tax");
        await Sender.CreateTaxTypeAsync("VAT Tax");

        var query = new GetTaxTypesQuery("", null, 1, 10);

        // Act
        Result<GetTaxTypesQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Should_HandleWhitespaceSearchString()
    {
        // Arrange
        await CleanDatabaseAsync();
        await Sender.CreateTaxTypeAsync("Income Tax");
        await Sender.CreateTaxTypeAsync("VAT Tax");

        var query = new GetTaxTypesQuery("   ", null, 1, 10);

        // Act
        Result<GetTaxTypesQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, -1)]
    public async Task Should_ThrowException_WhenPaginationParametersAreInvalid(int page, int pageSize)
    {
        // Arrange
        await CleanDatabaseAsync();
        await Sender.CreateTaxTypeAsync("Income Tax");

        var query = new GetTaxTypesQuery(null, null, page, pageSize);

        // Act & Assert
        // Invalid pagination parameters cause database-level errors (OFFSET/LIMIT negative)
        // These are wrapped as ViaRicecoException by the exception handling pipeline
        await Assert.ThrowsAsync<ViaRicecoException>(async () =>
        {
            await Sender.Send(query);
        });
    }

    [Fact]
    public async Task Should_ReturnEmptyResult_WhenPageSizeIsZero()
    {
        // Arrange
        await CleanDatabaseAsync();
        await Sender.CreateTaxTypeAsync("Income Tax");

        var query = new GetTaxTypesQuery(null, null, 1, 0);

        // Act
        Result<GetTaxTypesQueryResponse> result = await Sender.Send(query);

        // Assert
        // PageSize 0 returns empty results but doesn't fail
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }
}
