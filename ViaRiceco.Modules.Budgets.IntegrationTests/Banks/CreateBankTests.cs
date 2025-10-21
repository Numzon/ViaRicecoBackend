using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.Banks;

public sealed class CreateBankTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private sealed class CreateBankRequest
    {
        public string Name { get; init; } = string.Empty;
    }

    [Fact]
    public async Task Should_CreateBank_WhenRequestIsValid()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        var request = new CreateBankRequest
        {
            Name = Faker.Company.CompanyName()
        };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/budgets/banks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        string? locationHeader = response.Headers.Location?.ToString();
        locationHeader.Should().NotBeNull();
        locationHeader.Should().Contain("/api/budgets/banks/");
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenNameIsEmpty()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        var request = new CreateBankRequest
        {
            Name = string.Empty
        };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/budgets/banks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenNameIsNull()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        var request = new { Name = (string?)null };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/budgets/banks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_CreateBankWithUnicodeName()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        var request = new CreateBankRequest
        {
            Name = "Société Générale 中国银行 البنك الأهلي"
        };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/budgets/banks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        string? locationHeader = response.Headers.Location?.ToString();
        locationHeader.Should().NotBeNull();
        locationHeader.Should().Contain("/api/budgets/banks/");
    }

    [Theory]
    [InlineData("Chase Bank")]
    [InlineData("Bank of America")]
    [InlineData("Wells Fargo")]
    [InlineData("Citibank")]
    [InlineData("Goldman Sachs")]
    public async Task Should_CreateBank_WithCommonBankNames(string bankName)
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        var request = new CreateBankRequest
        {
            Name = bankName
        };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/budgets/banks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        string? locationHeader = response.Headers.Location?.ToString();
        locationHeader.Should().NotBeNull();
        locationHeader.Should().Contain("/api/budgets/banks/");
    }
}
