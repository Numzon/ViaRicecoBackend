using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Budgets.Domain.Banks;
using ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.Banks;

public sealed class UpdateBankTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private sealed class UpdateBankRequest
    {
        public string Name { get; init; } = string.Empty;
    }

    [Fact]
    public async Task Should_UpdateBank_WhenRequestIsValid()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        var bank = Bank.Create($"Original Bank Name - {Guid.NewGuid()}", DateTime.UtcNow);
        DbContext.Banks.Add(bank);
        await DbContext.SaveChangesAsync();
        
        var request = new UpdateBankRequest
        {
            Name = "Updated Bank Name"
        };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/banks/{bank.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify the update in the database
        DbContext.ChangeTracker.Clear();
        Bank? updatedBank = await DbContext.Banks.FirstOrDefaultAsync(b => b.Id == bank.Id);
        updatedBank.Should().NotBeNull();
        updatedBank!.Name.Should().Be("Updated Bank Name");
        updatedBank.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenBankDoesNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        string nonExistentBankId = "b_" + Guid.NewGuid();
        var request = new UpdateBankRequest
        {
            Name = "Updated Bank Name"
        };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/banks/{nonExistentBankId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenNameIsEmpty()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        var bank = Bank.Create($"Original Bank Name - {Guid.NewGuid()}", DateTime.UtcNow);
        DbContext.Banks.Add(bank);
        await DbContext.SaveChangesAsync();
        
        var request = new UpdateBankRequest
        {
            Name = string.Empty
        };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/banks/{bank.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        // Verify the bank was not updated
        DbContext.ChangeTracker.Clear();
        Bank? unchangedBank = await DbContext.Banks.FirstOrDefaultAsync(b => b.Id == bank.Id);
        unchangedBank.Should().NotBeNull();
        unchangedBank!.Name.Should().StartWith("Original Bank Name"); // Should start with original name
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenNameIsNull()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        var bank = Bank.Create($"Original Bank Name - {Guid.NewGuid()}", DateTime.UtcNow);
        DbContext.Banks.Add(bank);
        await DbContext.SaveChangesAsync();
        
        var request = new { Name = (string?)null };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/banks/{bank.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_UpdateBankWithUnicodeName()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        var bank = Bank.Create($"Original Bank Name - {Guid.NewGuid()}", DateTime.UtcNow);
        DbContext.Banks.Add(bank);
        await DbContext.SaveChangesAsync();
        
        var request = new UpdateBankRequest
        {
            Name = "Société Générale 中国银行 البنك الأهلي"
        };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/banks/{bank.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify the update in the database
        DbContext.ChangeTracker.Clear();
        Bank? updatedBank = await DbContext.Banks.FirstOrDefaultAsync(b => b.Id == bank.Id);
        updatedBank.Should().NotBeNull();
        updatedBank!.Name.Should().Be("Société Générale 中国银行 البنك الأهلي");
    }

    [Fact]
    public async Task Should_NotUpdateTimestamp_WhenNameIsTheSame()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        string bankName = $"Same Bank Name - {Guid.NewGuid()}";
        var bank = Bank.Create(bankName, DateTime.UtcNow);
        DbContext.Banks.Add(bank);
        await DbContext.SaveChangesAsync();
        
        DateTime? originalUpdatedAt = bank.UpdatedAtUtc;
        
        var request = new UpdateBankRequest
        {
            Name = bankName // Same name as created
        };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/banks/{bank.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify the timestamp was not updated
        DbContext.ChangeTracker.Clear();
        Bank? unchangedBank = await DbContext.Banks.FirstOrDefaultAsync(b => b.Id == bank.Id);
        unchangedBank.Should().NotBeNull();
        unchangedBank!.Name.Should().Be(bankName);
        unchangedBank.UpdatedAtUtc.Should().Be(originalUpdatedAt); // Should remain unchanged
    }

    [Theory]
    [InlineData("Chase Bank Updated")]
    [InlineData("Bank of America Updated")]
    [InlineData("Wells Fargo Updated")]
    [InlineData("Citibank Updated")]
    [InlineData("Goldman Sachs Updated")]
    public async Task Should_UpdateBank_WithCommonBankNames(string newBankName)
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        var bank = Bank.Create($"Original Bank Name - {Guid.NewGuid()}", DateTime.UtcNow);
        DbContext.Banks.Add(bank);
        await DbContext.SaveChangesAsync();
        
        var request = new UpdateBankRequest
        {
            Name = newBankName
        };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/banks/{bank.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify the update in the database
        DbContext.ChangeTracker.Clear();
        Bank? updatedBank = await DbContext.Banks.FirstOrDefaultAsync(b => b.Id == bank.Id);
        updatedBank.Should().NotBeNull();
        updatedBank!.Name.Should().Be(newBankName);
    }
}
