using FluentAssertions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Domain.Investments;
using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;
using ViaRiceco.Modules.Portfolios.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Portfolios.UnitTests.Investments;

public sealed class InvestmentTests : BaseTest
{
    [Fact]
    public void Create_Should_CreateInvestmentWithValidData()
    {
        // Arrange
        string name = "Apple Inc.";
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var investment = Investment.Create(name, investmentStrategyId, createdAtUtc);

        // Assert
        investment.Should().NotBeNull();
        investment.Id.Should().StartWith("i_");
        investment.Name.Should().Be(name);
        investment.InvestmentStrategyId.Should().Be(investmentStrategyId);
        investment.ModelPortfolioPercentage.Should().Be(0m); // Investments are created with 0% by default
        investment.RealPortfolioPercentage.Should().Be(0m);
        investment.CurrentAmount.Should().Be(0m);
        investment.CreatedAtUtc.Should().Be(createdAtUtc);
        investment.UpdatedAtUtc.Should().BeNull();
        investment.PurchaseRecords.Should().BeEmpty();
        investment.InvestedAmount.Should().Be(0m);
        investment.CurrentInvestedDifference.Should().Be(0m);
    }

    [Fact]
    public void Create_Should_GenerateUniqueIds()
    {
        // Arrange
        string name = "Apple Inc.";
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var investment1 = Investment.Create(name, investmentStrategyId, createdAtUtc);
        var investment2 = Investment.Create(name, investmentStrategyId, createdAtUtc);

        // Assert
        investment1.Id.Should().NotBe(investment2.Id);
        investment1.Id.Should().StartWith("i_");
        investment2.Id.Should().StartWith("i_");
    }

    [Fact]
    public void Create_Should_PublishInvestmentCreatedDomainEvent()
    {
        // Arrange
        string name = "Apple Inc.";
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var investment = Investment.Create(name, investmentStrategyId, createdAtUtc);

        // Assert
        InvestmentCreatedDomainEvent domainEvent = AssertDomainEventWasPublished<InvestmentCreatedDomainEvent>(investment);
        domainEvent.InvestmentId.Should().Be(investment.Id);
        domainEvent.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void Update_Should_UpdateNameAndModelPortfolioPercentage()
    {
        // Arrange
        string originalName = "Apple Inc.";
        string newName = "Apple Inc. (AAPL)";
        decimal newPercentage = 30.0m;
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var investment = Investment.Create(originalName, investmentStrategyId, createdAtUtc);

        // Act
        investment.Update(newName, newPercentage, updatedAtUtc);

        // Assert
        investment.Name.Should().Be(newName);
        investment.ModelPortfolioPercentage.Should().Be(newPercentage);
        investment.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_PublishInvestmentUpdatedDomainEvent()
    {
        // Arrange
        string originalName = "Apple Inc.";
        string newName = "Apple Inc. (AAPL)";
        decimal newPercentage = 30.0m;
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var investment = Investment.Create(originalName, investmentStrategyId, createdAtUtc);

        // Act
        investment.Update(newName, newPercentage, updatedAtUtc);

        // Assert
        InvestmentUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<InvestmentUpdatedDomainEvent>(investment);
        domainEvent.InvestmentId.Should().Be(investment.Id);
        domainEvent.Name.Should().Be(newName);
        domainEvent.ModelPortfolioPercentage.Should().Be(newPercentage);
        domainEvent.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void UpdateCurrentAmount_Should_UpdateCurrentAmountAndPublishEvent()
    {
        // Arrange
        string name = "Apple Inc.";
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        decimal newCurrentAmount = 5000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var investment = Investment.Create(name, investmentStrategyId, createdAtUtc);

        // Act
        investment.UpdateCurrentAmount(newCurrentAmount, updatedAtUtc);

        // Assert
        investment.CurrentAmount.Should().Be(newCurrentAmount);
        investment.UpdatedAtUtc.Should().Be(updatedAtUtc);
        
        InvestmentCurrentAmountUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<InvestmentCurrentAmountUpdatedDomainEvent>(investment);
        domainEvent.InvestmentId.Should().Be(investment.Id);
        domainEvent.CurrentAmount.Should().Be(newCurrentAmount);
        domainEvent.CurrentInvestedDifference.Should().Be(newCurrentAmount); // No purchases yet, so difference = current amount
        domainEvent.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void UpdateRealPortfolioPercentage_Should_UpdatePercentageAndPublishEvent()
    {
        // Arrange
        string name = "Apple Inc.";
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        decimal realPercentage = 28.3m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var investment = Investment.Create(name, investmentStrategyId, createdAtUtc);

        // Act
        investment.UpdateRealPortfolioPercentage(realPercentage, updatedAtUtc);

        // Assert
        investment.RealPortfolioPercentage.Should().Be(realPercentage);
        investment.UpdatedAtUtc.Should().Be(updatedAtUtc);
        
        InvestmentRealPortfolioPercentageUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<InvestmentRealPortfolioPercentageUpdatedDomainEvent>(investment);
        domainEvent.InvestmentId.Should().Be(investment.Id);
        domainEvent.RealPortfolioPercentage.Should().Be(realPercentage);
        domainEvent.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void AddPurchaseRecord_Should_AddRecordToCollectionAndUpdateInvestedAmount()
    {
        // Arrange
        string name = "Apple Inc.";
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;

        DateTime purchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal amount = 100m;
        decimal pricePerUnit = 150m;
        string currencyId = $"c_{Guid.NewGuid()}";
        DateTime purchaseCreatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var investment = Investment.Create(name, investmentStrategyId, createdAtUtc);

        // Act
        Result<PurchaseRecord> result = investment.AddPurchaseRecord(purchaseDate, amount, pricePerUnit, currencyId, 20000.0m, purchaseCreatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        PurchaseRecord record = result.Value;
        record.Should().NotBeNull();
        record.InvestmentId.Should().Be(investment.Id);
        investment.PurchaseRecords.Should().HaveCount(1);
        investment.PurchaseRecords.Should().Contain(record);
        investment.InvestedAmount.Should().Be(15000m); // 100 * 150
        investment.CurrentInvestedDifference.Should().Be(-15000m); // 0 - 15000 (current amount is still 0)
    }

    [Fact]
    public void AddPurchaseRecord_Should_PublishPurchaseRecordAddedToInvestmentDomainEvent()
    {
        // Arrange
        string name = "Apple Inc.";
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;

        DateTime purchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal amount = 100m;
        decimal pricePerUnit = 150m;
        string currencyId = $"c_{Guid.NewGuid()}";
        DateTime purchaseCreatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var investment = Investment.Create(name, investmentStrategyId, createdAtUtc);

        // Act
        Result<PurchaseRecord> result = investment.AddPurchaseRecord(purchaseDate, amount, pricePerUnit, currencyId, 20000.0m, purchaseCreatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        PurchaseRecordAddedToInvestmentDomainEvent domainEvent = AssertDomainEventWasPublished<PurchaseRecordAddedToInvestmentDomainEvent>(investment);
        domainEvent.InvestmentId.Should().Be(investment.Id);
        domainEvent.CreatedAtUtc.Should().Be(purchaseCreatedAtUtc);
    }

    [Fact]
    public void RemovePurchaseRecord_Should_RemoveRecordAndUpdateInvestedAmount()
    {
        // Arrange
        string name = "Apple Inc.";
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;

        DateTime purchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal amount = 100m;
        decimal pricePerUnit = 150m;
        string currencyId = $"c_{Guid.NewGuid()}";
        DateTime purchaseCreatedAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var investment = Investment.Create(name, investmentStrategyId, createdAtUtc);
        Result<PurchaseRecord> addResult = investment.AddPurchaseRecord(purchaseDate, amount, pricePerUnit, currencyId, 20000.0m, purchaseCreatedAtUtc);
        PurchaseRecord record = addResult.Value;

        // Act
        Result result = investment.RemovePurchaseRecord(record.Id, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        investment.PurchaseRecords.Should().BeEmpty();
        investment.InvestedAmount.Should().Be(0m);
        investment.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void RemovePurchaseRecord_Should_ReturnFailure_WhenRecordNotFound()
    {
        // Arrange
        string name = "Apple Inc.";
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        string nonExistentRecordId = $"pr_{Guid.NewGuid()}";

        var investment = Investment.Create(name, investmentStrategyId, createdAtUtc);

        // Act
        Result result = investment.RemovePurchaseRecord(nonExistentRecordId, updatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PurchaseRecordErrors.NotFound(nonExistentRecordId));
    }

    [Fact]
    public void RemovePurchaseRecord_Should_PublishPurchaseRecordRemovedFromInvestmentDomainEvent()
    {
        // Arrange
        string name = "Apple Inc.";
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;

        DateTime purchaseDate = Faker.Date.PastOffset().UtcDateTime;
        decimal amount = 100m;
        decimal pricePerUnit = 150m;
        string currencyId = $"c_{Guid.NewGuid()}";
        DateTime purchaseCreatedAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var investment = Investment.Create(name, investmentStrategyId, createdAtUtc);
        Result<PurchaseRecord> addResult = investment.AddPurchaseRecord(purchaseDate, amount, pricePerUnit, currencyId, 20000.0m, purchaseCreatedAtUtc);
        PurchaseRecord record = addResult.Value;

        // Act
        Result result = investment.RemovePurchaseRecord(record.Id, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        PurchaseRecordRemovedFromInvestmentDomainEvent domainEvent = AssertDomainEventWasPublished<PurchaseRecordRemovedFromInvestmentDomainEvent>(investment);
        domainEvent.InvestmentId.Should().Be(investment.Id);
        domainEvent.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void InvestedAmount_Should_CalculateCorrectly_WithMultiplePurchaseRecords()
    {
        // Arrange
        string name = "Apple Inc.";
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;

        string currencyId = $"c_{Guid.NewGuid()}";
        DateTime purchaseCreatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var investment = Investment.Create(name, investmentStrategyId, createdAtUtc);

        // Act - Add multiple purchase records
        investment.AddPurchaseRecord(Faker.Date.PastOffset().UtcDateTime, 100m, 150m, currencyId, 50000.0m, purchaseCreatedAtUtc).IsSuccess.Should().BeTrue(); // 15,000
        investment.AddPurchaseRecord(Faker.Date.PastOffset().UtcDateTime, 50m, 160m, currencyId, 50000.0m, purchaseCreatedAtUtc).IsSuccess.Should().BeTrue();  // 8,000
        investment.AddPurchaseRecord(Faker.Date.PastOffset().UtcDateTime, 75m, 140m, currencyId, 50000.0m, purchaseCreatedAtUtc).IsSuccess.Should().BeTrue();  // 10,500

        // Assert
        investment.PurchaseRecords.Should().HaveCount(3);
        investment.InvestedAmount.Should().Be(33500m); // 15,000 + 8,000 + 10,500
    }

    [Fact]
    public void CurrentInvestedDifference_Should_CalculateCorrectly()
    {
        // Arrange
        string name = "Apple Inc.";
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;

        string currencyId = $"c_{Guid.NewGuid()}";
        DateTime purchaseCreatedAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var investment = Investment.Create(name, investmentStrategyId, createdAtUtc);

        // Act
        investment.AddPurchaseRecord(Faker.Date.PastOffset().UtcDateTime, 100m, 150m, currencyId, 25000.0m, purchaseCreatedAtUtc); // Invested: 15,000
        investment.UpdateCurrentAmount(18000m, updatedAtUtc); // Current: 18,000

        // Assert
        investment.InvestedAmount.Should().Be(15000m);
        investment.CurrentAmount.Should().Be(18000m);
        investment.CurrentInvestedDifference.Should().Be(3000m); // 18,000 - 15,000 = +3,000 profit
    }

    [Fact]
    public void CurrentInvestedDifference_Should_ShowLoss_WhenCurrentAmountIsLower()
    {
        // Arrange
        string name = "Apple Inc.";
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;

        string currencyId = $"c_{Guid.NewGuid()}";
        DateTime purchaseCreatedAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var investment = Investment.Create(name, investmentStrategyId, createdAtUtc);

        // Act
        investment.AddPurchaseRecord(Faker.Date.PastOffset().UtcDateTime, 100m, 150m, currencyId, 25000.0m, purchaseCreatedAtUtc); // Invested: 15,000
        investment.UpdateCurrentAmount(12000m, updatedAtUtc); // Current: 12,000

        // Assert
        investment.InvestedAmount.Should().Be(15000m);
        investment.CurrentAmount.Should().Be(12000m);
        investment.CurrentInvestedDifference.Should().Be(-3000m); // 12,000 - 15,000 = -3,000 loss
    }

    [Theory]
    [InlineData("Apple Inc.")]
    [InlineData("Microsoft Corporation")]
    [InlineData("Amazon.com Inc.")]
    [InlineData("Tesla, Inc.")]
    [InlineData("S&P 500 ETF (SPY)")]
    public void Create_Should_HandleCommonInvestmentNames(string investmentName)
    {
        // Arrange
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var investment = Investment.Create(investmentName, investmentStrategyId, createdAtUtc);

        // Assert
        investment.Name.Should().Be(investmentName);
        investment.Id.Should().StartWith("i_");
    }

    [Fact]
    public void Create_Should_SetDefaultModelPortfolioPercentageToZero()
    {
        // Arrange
        string name = "Test Investment";
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var investment = Investment.Create(name, investmentStrategyId, createdAtUtc);

        // Assert
        investment.ModelPortfolioPercentage.Should().Be(0);
    }

    [Theory]
    [InlineData(0.01)]    // Very small percentage
    [InlineData(50.0)]    // Half portfolio
    [InlineData(99.99)]   // Nearly entire portfolio
    public void Update_Should_HandleVariousPortfolioPercentages(decimal percentage)
    {
        // Arrange
        string name = "Test Investment";
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.FutureOffset().UtcDateTime;

        // Act
        var investment = Investment.Create(name, investmentStrategyId, createdAtUtc);
        investment.Update(name, percentage, updatedAtUtc);

        // Assert
        investment.ModelPortfolioPercentage.Should().Be(percentage);
    }

    [Fact]
    public void AddPurchaseRecord_Should_HandleMultiplePurchasesOnSameDay()
    {
        // Arrange
        string name = "Apple Inc.";
        string investmentStrategyId = $"is_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;

        DateTime samePurchaseDate = Faker.Date.PastOffset().UtcDateTime;
        string currencyId = $"c_{Guid.NewGuid()}";
        DateTime purchaseCreatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var investment = Investment.Create(name, investmentStrategyId, createdAtUtc);

        // Act - Add multiple purchases on same day
        Result<PurchaseRecord> result1 = investment.AddPurchaseRecord(samePurchaseDate, 50m, 150m, currencyId, 20000.0m, purchaseCreatedAtUtc);
        Result<PurchaseRecord> result2 = investment.AddPurchaseRecord(samePurchaseDate, 50m, 150m, currencyId, 20000.0m, purchaseCreatedAtUtc);
        PurchaseRecord record1 = result1.Value;
        PurchaseRecord record2 = result2.Value;

        // Assert
        investment.PurchaseRecords.Should().HaveCount(2);
        investment.PurchaseRecords.Should().Contain(record1);
        investment.PurchaseRecords.Should().Contain(record2);
        investment.InvestedAmount.Should().Be(15000m); // (50 + 50) * 150
    }
}
