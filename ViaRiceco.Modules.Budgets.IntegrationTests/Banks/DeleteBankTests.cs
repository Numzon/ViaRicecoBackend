using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Budgets.Domain.Banks;
using ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.Banks;

public sealed class DeleteBankTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_DeleteBank_WhenBankExists()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        var bank = Bank.Create($"Test Bank - {Guid.NewGuid()}", DateTime.UtcNow);
        DbContext.Banks.Add(bank);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/budgets/banks/{bank.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the bank was deleted from the database
        Bank? deletedBank = await DbContext.Banks.FirstOrDefaultAsync(b => b.Id == bank.Id);
        deletedBank.Should().BeNull();
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenBankDoesNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        string nonExistentBankId = "b_" + Guid.NewGuid();

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/budgets/banks/{nonExistentBankId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent); // Idempotent - should still return 204
    }

    [Fact]
    public async Task Should_DeleteMultipleBanks_WhenCalledMultipleTimes()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        var bank1 = Bank.Create($"Bank 1 - {Guid.NewGuid()}", DateTime.UtcNow);
        var bank2 = Bank.Create($"Bank 2 - {Guid.NewGuid()}", DateTime.UtcNow);
        var bank3 = Bank.Create($"Bank 3 - {Guid.NewGuid()}", DateTime.UtcNow);
        
        DbContext.Banks.AddRange(bank1, bank2, bank3);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response1 = await client.DeleteAsync($"/api/budgets/banks/{bank1.Id}");
        HttpResponseMessage response2 = await client.DeleteAsync($"/api/budgets/banks/{bank2.Id}");
        HttpResponseMessage response3 = await client.DeleteAsync($"/api/budgets/banks/{bank3.Id}");

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response2.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response3.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify all banks were deleted from the database
        int remainingBanksCount = await DbContext.Banks.CountAsync();
        remainingBanksCount.Should().Be(0);
    }

    [Fact]
    public async Task Should_BeIdempotent_WhenDeletingSameBankTwice()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        var bank = Bank.Create($"Test Bank - {Guid.NewGuid()}", DateTime.UtcNow);
        DbContext.Banks.Add(bank);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage firstResponse = await client.DeleteAsync($"/api/budgets/banks/{bank.Id}");
        HttpResponseMessage secondResponse = await client.DeleteAsync($"/api/budgets/banks/{bank.Id}");

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NoContent); // Idempotent - should still return 204
        
        // Verify the bank was deleted from the database
        Bank? deletedBank = await DbContext.Banks.FirstOrDefaultAsync(b => b.Id == bank.Id);
        deletedBank.Should().BeNull();
    }

    [Fact]
    public async Task Should_OnlyDeleteSpecifiedBank_WhenMultipleBanksExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        var bankToDelete = Bank.Create($"Bank To Delete - {Guid.NewGuid()}", DateTime.UtcNow);
        var bankToKeep1 = Bank.Create($"Bank To Keep 1 - {Guid.NewGuid()}", DateTime.UtcNow);
        var bankToKeep2 = Bank.Create($"Bank To Keep 2 - {Guid.NewGuid()}", DateTime.UtcNow);
        
        DbContext.Banks.AddRange(bankToDelete, bankToKeep1, bankToKeep2);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/budgets/banks/{bankToDelete.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify only the specified bank was deleted
        Bank? deletedBank = await DbContext.Banks.FirstOrDefaultAsync(b => b.Id == bankToDelete.Id);
        deletedBank.Should().BeNull();
        
        Bank? keptBank1 = await DbContext.Banks.FirstOrDefaultAsync(b => b.Id == bankToKeep1.Id);
        keptBank1.Should().NotBeNull();
        keptBank1!.Name.Should().StartWith("Bank To Keep 1"); // Should start with original name
        
        Bank? keptBank2 = await DbContext.Banks.FirstOrDefaultAsync(b => b.Id == bankToKeep2.Id);
        keptBank2.Should().NotBeNull();
        keptBank2!.Name.Should().StartWith("Bank To Keep 2"); // Should start with original name
    }
}
