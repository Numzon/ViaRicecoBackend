using FluentAssertions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;
using ViaRiceco.Modules.Portfolios.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Portfolios.UnitTests.PurchaseRecords;

public sealed class PurchaseRecordTests : BaseTest
{
    [Fact]
    public void Create_Should_CreatePurchaseRecordWithValidData()
    {
        // Arrange
        DateTime purchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal amount = Faker.Random.Decimal(1, 1000);
        decimal pricePerUnit = Faker.Random.Decimal(10, 500);
        string currencyId = $"c_{Guid.NewGuid()}";
        string investmentId = $"i_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        decimal totalPrice = amount * pricePerUnit;
        Result<PurchaseRecord> result = PurchaseRecord.Create(purchaseDate, amount, pricePerUnit, currencyId, investmentId, totalPrice + 100m, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        PurchaseRecord record = result.Value;
        record.Should().NotBeNull();
        record.Id.Should().StartWith("pr_");
        record.PurchaseDate.Should().Be(purchaseDate);
        record.Amount.Should().Be(amount);
        record.PricePerUnit.Should().Be(pricePerUnit);
        record.TotalPrice.Should().Be(amount * pricePerUnit);
        record.CurrencyId.Should().Be(currencyId);
        record.InvestmentId.Should().Be(investmentId);
        record.CreatedAtUtc.Should().Be(createdAtUtc);
        record.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_Should_CalculateTotalPriceCorrectly()
    {
        // Arrange
        DateTime purchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal amount = 100m;
        decimal pricePerUnit = 25.50m;
        string currencyId = $"c_{Guid.NewGuid()}";
        string investmentId = $"i_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        decimal totalPrice = amount * pricePerUnit;
        Result<PurchaseRecord> result = PurchaseRecord.Create(purchaseDate, amount, pricePerUnit, currencyId, investmentId, totalPrice + 100m, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalPrice.Should().Be(2550m); // 100 * 25.50
    }

    [Fact]
    public void Create_Should_GenerateUniqueIds()
    {
        // Arrange
        DateTime purchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal amount = Faker.Random.Decimal(1, 1000);
        decimal pricePerUnit = Faker.Random.Decimal(10, 500);
        string currencyId = $"c_{Guid.NewGuid()}";
        string investmentId = $"i_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        decimal totalPrice = amount * pricePerUnit;
        Result<PurchaseRecord> result1 = PurchaseRecord.Create(purchaseDate, amount, pricePerUnit, currencyId, investmentId, totalPrice + 100m, createdAtUtc);
        Result<PurchaseRecord> result2 = PurchaseRecord.Create(purchaseDate, amount, pricePerUnit, currencyId, investmentId, totalPrice + 100m, createdAtUtc);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Id.Should().NotBe(result2.Value.Id);
        result1.Value.Id.Should().StartWith("pr_");
        result2.Value.Id.Should().StartWith("pr_");
    }

    [Fact]
    public void Create_Should_PublishPurchaseRecordCreatedDomainEvent()
    {
        // Arrange
        DateTime purchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal amount = Faker.Random.Decimal(1, 1000);
        decimal pricePerUnit = Faker.Random.Decimal(10, 500);
        string currencyId = $"c_{Guid.NewGuid()}";
        string investmentId = $"i_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        decimal totalPrice = amount * pricePerUnit;
        Result<PurchaseRecord> result = PurchaseRecord.Create(purchaseDate, amount, pricePerUnit, currencyId, investmentId, totalPrice + 100m, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        PurchaseRecord record = result.Value;
        PurchaseRecordCreatedDomainEvent domainEvent = AssertDomainEventWasPublished<PurchaseRecordCreatedDomainEvent>(record);
        domainEvent.RecordId.Should().Be(record.Id);
        domainEvent.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_Should_ReturnFailure_WhenAmountIsInvalid(decimal invalidAmount)
    {
        // Arrange
        DateTime purchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal pricePerUnit = Faker.Random.Decimal(10, 500);
        string currencyId = $"c_{Guid.NewGuid()}";
        string investmentId = $"i_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        decimal totalPrice = invalidAmount * pricePerUnit;
        Result<PurchaseRecord> result = PurchaseRecord.Create(purchaseDate, invalidAmount, pricePerUnit, currencyId, investmentId, Math.Max(totalPrice + 100m, 1000m), createdAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PurchaseRecordErrors.InvalidAmount(invalidAmount));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50.5)]
    public void Create_Should_ReturnFailure_WhenPricePerUnitIsInvalid(decimal invalidPricePerUnit)
    {
        // Arrange
        DateTime purchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal amount = Faker.Random.Decimal(1, 1000);
        string currencyId = $"c_{Guid.NewGuid()}";
        string investmentId = $"i_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        decimal totalPrice = amount * invalidPricePerUnit;
        Result<PurchaseRecord> result = PurchaseRecord.Create(purchaseDate, amount, invalidPricePerUnit, currencyId, investmentId, Math.Max(totalPrice + 100m, 1000m), createdAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PurchaseRecordErrors.InvalidPricePerUnit(invalidPricePerUnit));
    }

    [Fact]
    public void Create_Should_ReturnFailure_WhenPurchaseDateIsInFuture()
    {
        // Arrange
        DateTime futurePurchaseDate = Faker.Date.FutureOffset().UtcDateTime;
        decimal amount = Faker.Random.Decimal(1, 1000);
        decimal pricePerUnit = Faker.Random.Decimal(10, 500);
        string currencyId = $"c_{Guid.NewGuid()}";
        string investmentId = $"i_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        decimal totalPrice = amount * pricePerUnit;
        Result<PurchaseRecord> result = PurchaseRecord.Create(futurePurchaseDate, amount, pricePerUnit, currencyId, investmentId, totalPrice + 100m, createdAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PurchaseRecordErrors.FuturePurchaseDate(futurePurchaseDate));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_Should_ReturnFailure_WhenCurrencyIdIsInvalid(string? invalidCurrencyId)
    {
        // Arrange
        DateTime purchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal amount = Faker.Random.Decimal(1, 1000);
        decimal pricePerUnit = Faker.Random.Decimal(10, 500);
        string investmentId = $"i_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        decimal totalPrice = amount * pricePerUnit;
        Result<PurchaseRecord> result = PurchaseRecord.Create(purchaseDate, amount, pricePerUnit, invalidCurrencyId!, investmentId, totalPrice + 100m, createdAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PurchaseRecordErrors.InvalidCurrency(invalidCurrencyId!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_Should_ReturnFailure_WhenInvestmentIdIsInvalid(string? invalidInvestmentId)
    {
        // Arrange
        DateTime purchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal amount = Faker.Random.Decimal(1, 1000);
        decimal pricePerUnit = Faker.Random.Decimal(10, 500);
        string currencyId = $"c_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        decimal totalPrice = amount * pricePerUnit;
        Result<PurchaseRecord> result = PurchaseRecord.Create(purchaseDate, amount, pricePerUnit, currencyId, invalidInvestmentId!, totalPrice + 100m, createdAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PurchaseRecordErrors.InvalidInvestment(invalidInvestmentId!));
    }

    [Fact]
    public void Update_Should_UpdatePurchaseRecordWhenDataChanges()
    {
        // Arrange
        DateTime originalPurchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal originalAmount = 100m;
        decimal originalPricePerUnit = 25.50m;
        string originalCurrencyId = $"c_{Guid.NewGuid()}";
        string investmentId = $"i_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;

        DateTime newPurchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal newAmount = 150m;
        decimal newPricePerUnit = 30.75m;
        string newCurrencyId = $"c_{Guid.NewGuid()}";
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        Result<PurchaseRecord> createResult = PurchaseRecord.Create(originalPurchaseDate, originalAmount, originalPricePerUnit, originalCurrencyId, investmentId, 5000.0m, createdAtUtc);
        PurchaseRecord record = createResult.Value;

        // Act
        Result result = record.Update(newPurchaseDate, newAmount, newPricePerUnit, newCurrencyId, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        record.PurchaseDate.Should().Be(newPurchaseDate);
        record.Amount.Should().Be(newAmount);
        record.PricePerUnit.Should().Be(newPricePerUnit);
        record.TotalPrice.Should().Be(newAmount * newPricePerUnit); // 150 * 30.75 = 4612.5
        record.CurrencyId.Should().Be(newCurrencyId);
        record.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_PublishPurchaseRecordUpdatedDomainEvent_WhenDataChanges()
    {
        // Arrange
        DateTime originalPurchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal originalAmount = 100m;
        decimal originalPricePerUnit = 25.50m;
        string originalCurrencyId = $"c_{Guid.NewGuid()}";
        string investmentId = $"i_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;

        DateTime newPurchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal newAmount = 150m;
        decimal newPricePerUnit = 30.75m;
        string newCurrencyId = $"c_{Guid.NewGuid()}";
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        Result<PurchaseRecord> createResult = PurchaseRecord.Create(originalPurchaseDate, originalAmount, originalPricePerUnit, originalCurrencyId, investmentId, 5000.0m, createdAtUtc);
        PurchaseRecord record = createResult.Value;

        // Act
        Result result = record.Update(newPurchaseDate, newAmount, newPricePerUnit, newCurrencyId, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        PurchaseRecordUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<PurchaseRecordUpdatedDomainEvent>(record);
        domainEvent.RecordId.Should().Be(record.Id);
        domainEvent.PurchaseDate.Should().Be(newPurchaseDate);
        domainEvent.Amount.Should().Be(newAmount);
        domainEvent.PricePerUnit.Should().Be(newPricePerUnit);
        domainEvent.TotalPrice.Should().Be(newAmount * newPricePerUnit);
        domainEvent.CurrencyId.Should().Be(newCurrencyId);
        domainEvent.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_NotUpdateOrPublishEvent_WhenDataIsSame()
    {
        // Arrange
        DateTime purchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal amount = 100m;
        decimal pricePerUnit = 25.50m;
        string currencyId = $"c_{Guid.NewGuid()}";
        string investmentId = $"i_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        decimal totalPrice = amount * pricePerUnit;
        Result<PurchaseRecord> createResult = PurchaseRecord.Create(purchaseDate, amount, pricePerUnit, currencyId, investmentId, totalPrice + 100m, createdAtUtc);
        PurchaseRecord record = createResult.Value;

        // Act
        Result result = record.Update(purchaseDate, amount, pricePerUnit, currencyId, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        record.DomainEvents.OfType<PurchaseRecordUpdatedDomainEvent>().Should().BeEmpty();
        record.UpdatedAtUtc.Should().BeNull(); // Should not update timestamp
    }

    [Fact]
    public void Update_Should_ReturnFailure_WhenNewDataIsInvalid()
    {
        // Arrange
        DateTime originalPurchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal originalAmount = 100m;
        decimal originalPricePerUnit = 25.50m;
        string originalCurrencyId = $"c_{Guid.NewGuid()}";
        string investmentId = $"i_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        Result<PurchaseRecord> createResult = PurchaseRecord.Create(originalPurchaseDate, originalAmount, originalPricePerUnit, originalCurrencyId, investmentId, 5000.0m, createdAtUtc);
        PurchaseRecord record = createResult.Value;

        // Act - Try to update with invalid amount
        Result result = record.Update(originalPurchaseDate, -100m, originalPricePerUnit, originalCurrencyId, updatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PurchaseRecordErrors.InvalidAmount(-100m));
        record.Amount.Should().Be(originalAmount); // Should remain unchanged
    }

    [Fact]
    public void Create_Should_HandleVerySmallAmounts()
    {
        // Arrange
        DateTime purchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal amount = 0.0001m; // Very small but positive
        decimal pricePerUnit = 1000m;
        string currencyId = $"c_{Guid.NewGuid()}";
        string investmentId = $"i_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        decimal totalPrice = amount * pricePerUnit;
        Result<PurchaseRecord> result = PurchaseRecord.Create(purchaseDate, amount, pricePerUnit, currencyId, investmentId, totalPrice + 100m, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(amount);
        result.Value.TotalPrice.Should().Be(0.1m); // 0.0001 * 1000
    }

    [Fact]
    public void Create_Should_HandleVeryLargeAmounts()
    {
        // Arrange
        DateTime purchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal amount = 999999999m; // Very large amount
        decimal pricePerUnit = 1m;
        string currencyId = $"c_{Guid.NewGuid()}";
        string investmentId = $"i_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        decimal totalPrice = amount * pricePerUnit;
        Result<PurchaseRecord> result = PurchaseRecord.Create(purchaseDate, amount, pricePerUnit, currencyId, investmentId, totalPrice + 100m, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(amount);
        result.Value.TotalPrice.Should().Be(amount);
    }

    [Fact]
    public void Update_Should_HandleMultipleUpdates()
    {
        // Arrange
        DateTime originalPurchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal originalAmount = 100m;
        decimal originalPricePerUnit = 10m;
        string originalCurrencyId = $"c_{Guid.NewGuid()}";
        string investmentId = $"i_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        
        DateTime firstUpdateDate = Faker.Date.PastOffset().UtcDateTime;
        DateTime secondUpdateDate = Faker.Date.PastOffset().UtcDateTime;
        DateTime firstUpdatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        DateTime secondUpdatedAtUtc = Faker.Date.FutureOffset().UtcDateTime;

        Result<PurchaseRecord> createResult = PurchaseRecord.Create(originalPurchaseDate, originalAmount, originalPricePerUnit, originalCurrencyId, investmentId, 5000.0m, createdAtUtc);
        PurchaseRecord record = createResult.Value;

        // Act
        Result result1 = record.Update(firstUpdateDate, 150m, 15m, originalCurrencyId, firstUpdatedAtUtc);
        Result result2 = record.Update(secondUpdateDate, 200m, 20m, originalCurrencyId, secondUpdatedAtUtc);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        record.PurchaseDate.Should().Be(secondUpdateDate);
        record.Amount.Should().Be(200m);
        record.PricePerUnit.Should().Be(20m);
        record.TotalPrice.Should().Be(4000m); // 200 * 20
        record.UpdatedAtUtc.Should().Be(secondUpdatedAtUtc);
        
        // Should have 3 domain events: 1 created + 2 updated
        record.DomainEvents.Should().HaveCount(3);
        record.DomainEvents.OfType<PurchaseRecordCreatedDomainEvent>().Should().HaveCount(1);
        record.DomainEvents.OfType<PurchaseRecordUpdatedDomainEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void Create_Should_ReturnFailure_WhenInsufficientUninvestedAmount()
    {
        // Arrange
        DateTime purchaseDate = DateTime.UtcNow.AddDays(-1);
        decimal amount = 10.0m;
        decimal pricePerUnit = 100.0m; // Total cost: 1000
        string currencyId = $"c_{Guid.NewGuid()}";
        string investmentId = $"i_{Guid.NewGuid()}";
        decimal uninvestedAmount = 500.0m; // Less than total cost
        DateTime createdAtUtc = DateTime.UtcNow;

        // Act
        Result<PurchaseRecord> result = PurchaseRecord.Create(purchaseDate, amount, pricePerUnit, currencyId, investmentId, uninvestedAmount, createdAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PurchaseRecord.InsufficientUninvestedAmount");
    }

    [Fact]
    public void Create_Should_Succeed_WhenSufficientUninvestedAmount()
    {
        // Arrange
        DateTime purchaseDate = DateTime.UtcNow.AddDays(-1);
        decimal amount = 10.0m;
        decimal pricePerUnit = 100.0m; // Total cost: 1000
        string currencyId = $"c_{Guid.NewGuid()}";
        string investmentId = $"i_{Guid.NewGuid()}";
        decimal uninvestedAmount = 1500.0m; // More than total cost
        DateTime createdAtUtc = DateTime.UtcNow;

        // Act
        Result<PurchaseRecord> result = PurchaseRecord.Create(purchaseDate, amount, pricePerUnit, currencyId, investmentId, uninvestedAmount, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalPrice.Should().Be(1000.0m);
    }
}
