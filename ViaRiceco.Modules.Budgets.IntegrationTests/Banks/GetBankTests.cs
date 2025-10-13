using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Budgets.Domain.Banks;
using ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.Banks;

public sealed class GetBankTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnBank_WhenBankExists()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        var bank = Bank.Create($"Test Bank - {Guid.NewGuid()}", DateTime.UtcNow);
        DbContext.Banks.Add(bank);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/banks/{bank.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        string content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Test Bank");
        content.Should().Contain(bank.Id);
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenBankDoesNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        string nonExistentBankId = "b_" + Guid.NewGuid();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/banks/{nonExistentBankId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnBankWithHateoasLinks_WhenAcceptHeaderIncludesHateoas()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.via-riceco.hateoas.1+json"));
        
        var bank = Bank.Create($"Test Bank - {Guid.NewGuid()}", DateTime.UtcNow);
        DbContext.Banks.Add(bank);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/banks/{bank.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        string content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("_links");
        content.Should().Contain("self");
        content.Should().Contain("update");
        content.Should().Contain("delete");
    }

    [Fact]
    public async Task Should_ReturnBankWithoutHateoasLinks_WhenAcceptHeaderIsStandardJson()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        var bank = Bank.Create($"Test Bank - {Guid.NewGuid()}", DateTime.UtcNow);
        DbContext.Banks.Add(bank);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/banks/{bank.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        string content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("_links");
        content.Should().Contain("Test Bank");
        content.Should().Contain(bank.Id);
    }

    [Fact]
    public async Task Should_ReturnBankWithCorrectContentType()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        var bank = Bank.Create($"Test Bank - {Guid.NewGuid()}", DateTime.UtcNow);
        DbContext.Banks.Add(bank);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/banks/{bank.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }
}
