using System.Globalization;
using FluentAssertions;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Domain.Currencies;
using ViaRiceco.Modules.Portfolios.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Portfolios.UnitTests.Currencies;

public sealed class CurrencyTests : BaseTest
{
    [Fact]
    public void Create_Should_CreateCurrencyWithValidData()
    {
        // Arrange
        string name = "US Dollar";
        string code = "USD";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<Currency> result = Currency.Create(name, code, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Currency currency = result.Value;
        currency.Should().NotBeNull();
        currency.Id.Should().StartWith("c_");
        currency.Name.Should().Be(name);
        currency.Code.Should().Be(code.ToUpper(CultureInfo.InvariantCulture));
        currency.CreatedAtUtc.Should().Be(createdAtUtc);
        currency.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_Should_NormalizeCurrencyCodeToUpperCase()
    {
        // Arrange
        string name = "Euro";
        string code = "eur";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<Currency> result = Currency.Create(name, code, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be("EUR");
    }

    [Fact]
    public void Create_Should_GenerateUniqueIds()
    {
        // Arrange
        string name = "US Dollar";
        string code = "USD";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<Currency> result1 = Currency.Create(name, code, createdAtUtc);
        Result<Currency> result2 = Currency.Create(name, code, createdAtUtc);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Id.Should().NotBe(result2.Value.Id);
        result1.Value.Id.Should().StartWith("c_");
        result2.Value.Id.Should().StartWith("c_");
    }

    [Fact]
    public void Create_Should_PublishCurrencyCreatedDomainEvent()
    {
        // Arrange
        string name = "US Dollar";
        string code = "USD";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<Currency> result = Currency.Create(name, code, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Currency currency = result.Value;
        CurrencyCreatedDomainEvent domainEvent = AssertDomainEventWasPublished<CurrencyCreatedDomainEvent>(currency);
        domainEvent.CurrencyId.Should().Be(currency.Id);
        domainEvent.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Theory]
    [InlineData("", "USD")]
    [InlineData("   ", "USD")]
    [InlineData(null, "USD")]
    [InlineData("US Dollar", "")]
    [InlineData("US Dollar", "   ")]
    [InlineData("US Dollar", null)]
    public void Create_Should_ReturnFailure_WhenNameOrCodeIsInvalid(string? name, string? code)
    {
        // Arrange
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<Currency> result = Currency.Create(name!, code!, createdAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData("US")]   // Too short
    [InlineData("USDA")] // Too long
    [InlineData("US1")]  // Contains digit
    [InlineData("U$D")]  // Contains special character
    public void Create_Should_ReturnFailure_WhenCodeFormatIsInvalid(string invalidCode)
    {
        // Arrange
        string name = "US Dollar";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<Currency> result = Currency.Create(name, invalidCode, createdAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Validation("Currency.InvalidCode", "Currency code must be exactly 3 letters."));
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("JPY")]
    [InlineData("CHF")]
    public void Create_Should_HandleCommonCurrencyCodes(string currencyCode)
    {
        // Arrange
        string name = Faker.Finance.Currency().Description;
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<Currency> result = Currency.Create(name, currencyCode, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be(currencyCode.ToUpper(CultureInfo.InvariantCulture));
        result.Value.Id.Should().StartWith("c_");
    }

    [Fact]
    public void Update_Should_UpdateNameAndCodeWhenDifferent()
    {
        // Arrange
        string originalName = "US Dollar";
        string originalCode = "USD";
        string newName = "United States Dollar";
        string newCode = "USD";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        Result<Currency> createResult = Currency.Create(originalName, originalCode, createdAtUtc);
        Currency currency = createResult.Value;

        // Act
        Result result = currency.Update(newName, newCode, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        currency.Name.Should().Be(newName);
        currency.Code.Should().Be(newCode.ToUpper(CultureInfo.InvariantCulture));
        currency.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_PublishCurrencyUpdatedDomainEvent_WhenDataChanges()
    {
        // Arrange
        string originalName = "US Dollar";
        string originalCode = "USD";
        string newName = "United States Dollar";
        string newCode = "USD";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        Result<Currency> createResult = Currency.Create(originalName, originalCode, createdAtUtc);
        Currency currency = createResult.Value;

        // Act
        Result result = currency.Update(newName, newCode, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        CurrencyUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<CurrencyUpdatedDomainEvent>(currency);
        domainEvent.CurrencyId.Should().Be(currency.Id);
        domainEvent.Name.Should().Be(newName);
        domainEvent.Code.Should().Be(newCode.ToUpper(CultureInfo.InvariantCulture));
        domainEvent.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_NotUpdateOrPublishEvent_WhenDataIsSame()
    {
        // Arrange
        string name = "US Dollar";
        string code = "USD";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        Result<Currency> createResult = Currency.Create(name, code, createdAtUtc);
        Currency currency = createResult.Value;

        // Act
        Result result = currency.Update(name, code, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        currency.DomainEvents.OfType<CurrencyUpdatedDomainEvent>().Should().BeEmpty();
        currency.UpdatedAtUtc.Should().BeNull(); // Should not update timestamp
    }

    [Theory]
    [InlineData("", "USD")]
    [InlineData("   ", "USD")]
    [InlineData(null, "USD")]
    [InlineData("US Dollar", "")]
    [InlineData("US Dollar", "   ")]
    [InlineData("US Dollar", null)]
    public void Update_Should_ReturnFailure_WhenNewDataIsInvalid(string? newName, string? newCode)
    {
        // Arrange
        string originalName = "US Dollar";
        string originalCode = "USD";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        Result<Currency> createResult = Currency.Create(originalName, originalCode, createdAtUtc);
        Currency currency = createResult.Value;

        // Act
        Result result = currency.Update(newName!, newCode!, updatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        currency.Name.Should().Be(originalName); // Should remain unchanged
        currency.Code.Should().Be(originalCode); // Should remain unchanged
    }

    [Fact]
    public void Update_Should_HandleMultipleUpdates()
    {
        // Arrange
        string originalName = "US Dollar";
        string originalCode = "USD";
        string secondName = "United States Dollar";
        string secondCode = "USD";
        string finalName = "American Dollar";
        string finalCode = "USD";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime firstUpdateUtc = Faker.Date.RecentOffset().UtcDateTime;
        DateTime secondUpdateUtc = Faker.Date.FutureOffset().UtcDateTime;

        Result<Currency> createResult = Currency.Create(originalName, originalCode, createdAtUtc);
        Currency currency = createResult.Value;

        // Act
        Result result1 = currency.Update(secondName, secondCode, firstUpdateUtc);
        Result result2 = currency.Update(finalName, finalCode, secondUpdateUtc);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        currency.Name.Should().Be(finalName);
        currency.Code.Should().Be(finalCode);
        currency.UpdatedAtUtc.Should().Be(secondUpdateUtc);
        
        // Should have 3 domain events: 1 created + 2 updated
        currency.DomainEvents.Should().HaveCount(3);
        currency.DomainEvents.OfType<CurrencyCreatedDomainEvent>().Should().HaveCount(1);
        currency.DomainEvents.OfType<CurrencyUpdatedDomainEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void Create_Should_HandleLongCurrencyNames()
    {
        // Arrange
        string longName = "Some Very Long Currency Name That Exceeds Normal Length";
        string code = "ABC";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<Currency> result = Currency.Create(longName, code, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(longName);
    }

    [Fact]
    public void Update_Should_HandleCaseSensitiveChanges()
    {
        // Arrange
        string originalName = "us dollar";
        string originalCode = "usd";
        string newName = "US Dollar"; // Same but different case
        string newCode = "USD";       // Same but different case
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        Result<Currency> createResult = Currency.Create(originalName, originalCode, createdAtUtc);
        Currency currency = createResult.Value;

        // Act
        Result result = currency.Update(newName, newCode, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        currency.Name.Should().Be(newName);
        currency.Code.Should().Be(newCode.ToUpper(CultureInfo.InvariantCulture));
        currency.UpdatedAtUtc.Should().Be(updatedAtUtc);
        
        // Should publish update event since name is different (code will be same after normalization)
        CurrencyUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<CurrencyUpdatedDomainEvent>(currency);
        domainEvent.Name.Should().Be(newName);
    }
}
