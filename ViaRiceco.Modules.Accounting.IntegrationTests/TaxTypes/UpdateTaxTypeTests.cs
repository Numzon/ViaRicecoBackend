using FluentAssertions;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.UpdateTaxType;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;
using ViaRiceco.Modules.Accounting.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Accounting.IntegrationTests.TaxTypes;

public sealed class UpdateTaxTypeTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_UpdateTaxType_WhenCommandIsValid()
    {
        // Arrange
        await CleanDatabaseAsync();
        string originalName = "Original Tax";
        string updatedName = "Updated Tax Name";
        string taxTypeId = await Sender.CreateTaxTypeAsync(originalName);

        var command = new UpdateTaxTypeCommand(taxTypeId, updatedName);

        // Act
        Result<TaxTypeDto> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(taxTypeId);
        result.Value.Name.Should().Be(updatedName);

        // Verify database is updated
        TaxType? updatedTaxType = await DbContext.TaxTypes.FindAsync(taxTypeId);
        updatedTaxType.Should().NotBeNull();
        updatedTaxType!.Name.Should().Be(updatedName);
        updatedTaxType.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenTaxTypeDoesNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        string nonExistentId = $"tt_{Guid.NewGuid()}";
        var command = new UpdateTaxTypeCommand(nonExistentId, "Some Name");

        // Act
        Result<TaxTypeDto> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaxTypeErrors.NotFound(nonExistentId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_ReturnFailure_WhenNameIsInvalid(string invalidName)
    {
        // Arrange
        await CleanDatabaseAsync();
        string taxTypeId = await Sender.CreateTaxTypeAsync("Valid Tax");
        var command = new UpdateTaxTypeCommand(taxTypeId, invalidName);

        // Act
        Result<TaxTypeDto> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenIdIsInvalid()
    {
        // Arrange
        await CleanDatabaseAsync();
        var command = new UpdateTaxTypeCommand("", "Valid Name");

        // Act
        Result<TaxTypeDto> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenNameAlreadyExistsForDifferentTaxType()
    {
        // Arrange
        await CleanDatabaseAsync();
        string existingName = "Income Tax";
        await Sender.CreateTaxTypeAsync(existingName);
        string taxType2Id = await Sender.CreateTaxTypeAsync("VAT Tax");

        var command = new UpdateTaxTypeCommand(taxType2Id, existingName);

        // Act
        Result<TaxTypeDto> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaxTypeErrors.NameNotUnique(existingName));

        // Verify database is not updated
        TaxType? unchangedTaxType = await DbContext.TaxTypes.FindAsync(taxType2Id);
        unchangedTaxType.Should().NotBeNull();
        unchangedTaxType!.Name.Should().Be("VAT Tax");
        unchangedTaxType.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task Should_AllowSameNameForSameTaxType()
    {
        // Arrange
        await CleanDatabaseAsync();
        string taxTypeName = "Income Tax";
        string taxTypeId = await Sender.CreateTaxTypeAsync(taxTypeName);

        var command = new UpdateTaxTypeCommand(taxTypeId, taxTypeName);

        // Act
        Result<TaxTypeDto> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(taxTypeName);
    }

    [Fact]
    public async Task Should_UpdateOnlySpecifiedTaxType_WhenMultipleTaxTypesExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        string taxType1Id = await Sender.CreateTaxTypeAsync("Income Tax");
        string taxType2Id = await Sender.CreateTaxTypeAsync("VAT Tax");
        string taxType3Id = await Sender.CreateTaxTypeAsync("Corporate Tax");

        string newName = "Updated VAT Tax";
        var command = new UpdateTaxTypeCommand(taxType2Id, newName);

        // Act
        Result<TaxTypeDto> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify only the specified tax type is updated
        TaxType? updatedTaxType = await DbContext.TaxTypes.FindAsync(taxType2Id);
        updatedTaxType.Should().NotBeNull();
        updatedTaxType!.Name.Should().Be(newName);

        // Verify other tax types remain unchanged
        TaxType? unchangedTaxType1 = await DbContext.TaxTypes.FindAsync(taxType1Id);
        TaxType? unchangedTaxType3 = await DbContext.TaxTypes.FindAsync(taxType3Id);
        unchangedTaxType1!.Name.Should().Be("Income Tax");
        unchangedTaxType3!.Name.Should().Be("Corporate Tax");
        unchangedTaxType1.UpdatedAtUtc.Should().BeNull();
        unchangedTaxType3.UpdatedAtUtc.Should().BeNull();
    }

    [Theory]
    [InlineData("Simple Tax")]
    [InlineData("Tax with Numbers 123")]
    [InlineData("Tax & Fees")]
    [InlineData("Tax (Special Rate)")]
    public async Task Should_UpdateTaxType_WithDifferentNameFormats(string newName)
    {
        // Arrange
        await CleanDatabaseAsync();
        string taxTypeId = await Sender.CreateTaxTypeAsync("Original Name");
        var command = new UpdateTaxTypeCommand(taxTypeId, newName);

        // Act
        Result<TaxTypeDto> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(newName);

        // Verify database is updated
        TaxType? updatedTaxType = await DbContext.TaxTypes.FindAsync(taxTypeId);
        updatedTaxType!.Name.Should().Be(newName);
    }

    [Fact]
    public async Task Should_HandleCaseChangesInName()
    {
        // Arrange
        await CleanDatabaseAsync();
        string originalName = "income tax";
        string updatedName = "Income Tax";
        string taxTypeId = await Sender.CreateTaxTypeAsync(originalName);

        var command = new UpdateTaxTypeCommand(taxTypeId, updatedName);

        // Act
        Result<TaxTypeDto> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(updatedName);

        // Verify database reflects the case change
        TaxType? updatedTaxType = await DbContext.TaxTypes.FindAsync(taxTypeId);
        updatedTaxType!.Name.Should().Be(updatedName);
    }
}
