using FluentAssertions;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.DeleteTaxType;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;
using ViaRiceco.Modules.Accounting.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Accounting.IntegrationTests.TaxTypes;

public sealed class DeleteTaxTypeTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_DeleteTaxType_WhenTaxTypeExists()
    {
        // Arrange
        await CleanDatabaseAsync();
        string taxTypeId = await Sender.CreateTaxTypeAsync(Faker.Commerce.Categories(1)[0]);

        var command = new DeleteTaxTypeCommand(taxTypeId);

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verify tax type is deleted from database
        TaxType? deletedTaxType = await DbContext.TaxTypes.FindAsync(taxTypeId);
        deletedTaxType.Should().BeNull();
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenTaxTypeDoesNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        string nonExistentId = $"tt_{Guid.NewGuid()}";
        var command = new DeleteTaxTypeCommand(nonExistentId);

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaxTypeErrors.NotFound(nonExistentId));
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenCommandIsNotValid()
    {
        // Arrange
        await CleanDatabaseAsync();
        var command = new DeleteTaxTypeCommand("");

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Should_DeleteOnlySpecifiedTaxType_WhenMultipleTaxTypesExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        string taxType1Id = await Sender.CreateTaxTypeAsync("Income Tax");
        string taxType2Id = await Sender.CreateTaxTypeAsync("VAT Tax");
        string taxType3Id = await Sender.CreateTaxTypeAsync("Corporate Tax");

        var command = new DeleteTaxTypeCommand(taxType2Id);

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify only the specified tax type is deleted
        TaxType? deletedTaxType = await DbContext.TaxTypes.FindAsync(taxType2Id);
        deletedTaxType.Should().BeNull();

        // Verify other tax types still exist
        TaxType? remainingTaxType1 = await DbContext.TaxTypes.FindAsync(taxType1Id);
        TaxType? remainingTaxType3 = await DbContext.TaxTypes.FindAsync(taxType3Id);
        remainingTaxType1.Should().NotBeNull();
        remainingTaxType3.Should().NotBeNull();
    }

    [Theory]
    [InlineData("tt_")]
    [InlineData("invalid_id")]
    [InlineData("123")]
    public async Task Should_ReturnFailure_WhenIdFormatIsInvalid(string invalidId)
    {
        // Arrange
        await CleanDatabaseAsync();
        var command = new DeleteTaxTypeCommand(invalidId);

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaxTypeErrors.NotFound(invalidId));
    }
}
