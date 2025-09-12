using FluentAssertions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Domain.Investments;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;
using ViaRiceco.Modules.Portfolios.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Portfolios.UnitTests.InvestmentStrategies;

public sealed class InvestmentStrategyTests : BaseTest
{
    [Fact]
    public void Create_Should_CreateInvestmentStrategyWithValidData()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 10000m;
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);

        // Assert
        strategy.Should().NotBeNull();
        strategy.Id.Should().StartWith("is_");
        strategy.FinancialGoalId.Should().Be(financialGoalId);
        strategy.InvestmentStrategyTypeId.Should().Be(investmentStrategyTypeId);
        strategy.UninvestedAmount.Should().Be(uninvestedAmount);
        strategy.CreatedAtUtc.Should().Be(createdAtUtc);
        strategy.UpdatedAtUtc.Should().BeNull();
        strategy.Investments.Should().BeEmpty();
        strategy.TotalInvestedAmount.Should().Be(0m);
        strategy.TotalCurrentAmount.Should().Be(0m);
        strategy.TotalAmount.Should().Be(uninvestedAmount); // Only uninvested amount initially
    }

    [Fact]
    public void Create_Should_GenerateUniqueIds()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 10000m;
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var strategy1 = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);
        var strategy2 = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);

        // Assert
        strategy1.Id.Should().NotBe(strategy2.Id);
        strategy1.Id.Should().StartWith("is_");
        strategy2.Id.Should().StartWith("is_");
    }

    [Fact]
    public void Create_Should_PublishInvestmentStrategyCreatedDomainEvent()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 10000m;
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);

        // Assert
        InvestmentStrategyCreatedDomainEvent domainEvent = AssertDomainEventWasPublished<InvestmentStrategyCreatedDomainEvent>(strategy);
        domainEvent.StrategyId.Should().Be(strategy.Id);
        domainEvent.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void UpdateUninvestedAmount_Should_UpdateAmountWhenDifferent()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal originalAmount = 10000m;
        decimal newAmount = 15000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, originalAmount, createdAtUtc);

        // Act
        strategy.UpdateUninvestedAmount(newAmount, updatedAtUtc);

        // Assert
        strategy.UninvestedAmount.Should().Be(newAmount);
        strategy.UpdatedAtUtc.Should().Be(updatedAtUtc);
        strategy.TotalAmount.Should().Be(newAmount); // No investments yet
    }

    [Fact]
    public void UpdateUninvestedAmount_Should_PublishEvent_WhenAmountChanges()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal originalAmount = 10000m;
        decimal newAmount = 15000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, originalAmount, createdAtUtc);

        // Act
        strategy.UpdateUninvestedAmount(newAmount, updatedAtUtc);

        // Assert
        InvestmentStrategyUninvestedAmountUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<InvestmentStrategyUninvestedAmountUpdatedDomainEvent>(strategy);
        domainEvent.StrategyId.Should().Be(strategy.Id);
        domainEvent.UninvestedAmount.Should().Be(newAmount);
        domainEvent.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void UpdateUninvestedAmount_Should_NotUpdateOrPublishEvent_WhenAmountIsSame()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal amount = 10000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, amount, createdAtUtc);

        // Act
        strategy.UpdateUninvestedAmount(amount, updatedAtUtc);

        // Assert
        strategy.DomainEvents.OfType<InvestmentStrategyUninvestedAmountUpdatedDomainEvent>().Should().BeEmpty();
        strategy.UpdatedAtUtc.Should().BeNull(); // Should not update timestamp
    }

    [Fact]
    public void AddInvestment_Should_AddInvestmentToCollection()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 10000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;

        string investmentName = "Apple Inc.";
        DateTime investmentCreatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);

        // Act
        Result<Investment> addResult = strategy.AddInvestment(investmentName, investmentCreatedAtUtc);
        addResult.IsSuccess.Should().BeTrue();
        Investment investment = addResult.Value;

        // Assert
        investment.Should().NotBeNull();
        investment.InvestmentStrategyId.Should().Be(strategy.Id);
        strategy.Investments.Should().HaveCount(1);
        strategy.Investments.Should().Contain(investment);
        strategy.UpdatedAtUtc.Should().Be(investmentCreatedAtUtc);
    }

    [Fact]
    public void AddInvestment_Should_PublishInvestmentAddedToStrategyDomainEvent()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 10000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;

        string investmentName = "Apple Inc.";
        DateTime investmentCreatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);

        // Act
        strategy.AddInvestment(investmentName, investmentCreatedAtUtc);

        // Assert
        InvestmentAddedToStrategyDomainEvent domainEvent = AssertDomainEventWasPublished<InvestmentAddedToStrategyDomainEvent>(strategy);
        domainEvent.StrategyId.Should().Be(strategy.Id);
        domainEvent.CreatedAtUtc.Should().Be(investmentCreatedAtUtc);
    }

    [Fact]
    public void AddInvestment_Should_ThrowException_WhenTotalModelPercentageExceeds100()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 10000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime investmentCreatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);

        // Add investments up to 70%
        Result<Investment> result1 = strategy.AddInvestment("Apple Inc.", investmentCreatedAtUtc);
        Result<Investment> result2 = strategy.AddInvestment("Microsoft", investmentCreatedAtUtc);
        
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();

        // Act - Adding 40% more should exceed 100%
        Result<Investment> result = strategy.AddInvestment("Google", investmentCreatedAtUtc);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("InvestmentStrategy.ModelPortfolioPercentageExceeds100");
    }

    [Fact]
    public void RemoveInvestment_Should_RemoveInvestmentFromCollection()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 10000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime investmentCreatedAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);
        Result<Investment> addResult = strategy.AddInvestment("Apple Inc.", investmentCreatedAtUtc);
        addResult.IsSuccess.Should().BeTrue();
        Investment investment = addResult.Value;

        // Act
        Result result = strategy.RemoveInvestment(investment.Id, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        strategy.Investments.Should().BeEmpty();
        strategy.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void RemoveInvestment_Should_ReturnFailure_WhenInvestmentNotFound()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 10000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        string nonExistentInvestmentId = $"i_{Guid.NewGuid()}";

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);

        // Act
        Result result = strategy.RemoveInvestment(nonExistentInvestmentId, updatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvestmentStrategyErrors.InvestmentNotFound(nonExistentInvestmentId));
    }

    [Fact]
    public void RemoveInvestment_Should_PublishInvestmentRemovedFromStrategyDomainEvent()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 10000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime investmentCreatedAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);
        Result<Investment> addResult = strategy.AddInvestment("Apple Inc.", investmentCreatedAtUtc);
        addResult.IsSuccess.Should().BeTrue();
        Investment investment = addResult.Value;

        // Act
        Result result = strategy.RemoveInvestment(investment.Id, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        InvestmentRemovedFromStrategyDomainEvent domainEvent = AssertDomainEventWasPublished<InvestmentRemovedFromStrategyDomainEvent>(strategy);
        domainEvent.StrategyId.Should().Be(strategy.Id);
        domainEvent.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void UpdateInvestmentsModelPercentages_Should_UpdateMultipleInvestments()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 10000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime investmentCreatedAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);
        Result<Investment> addResult1 = strategy.AddInvestment("Apple Inc.", investmentCreatedAtUtc);
        Result<Investment> addResult2 = strategy.AddInvestment("Microsoft", investmentCreatedAtUtc);
        addResult1.IsSuccess.Should().BeTrue();
        addResult2.IsSuccess.Should().BeTrue();
        Investment investment1 = addResult1.Value;
        Investment investment2 = addResult2.Value;

        var newPercentages = new Dictionary<string, decimal>
        {
            { investment1.Id, 35m },
            { investment2.Id, 30m }
        };

        // Act
        Result result = strategy.UpdateInvestmentsModelPercentages(newPercentages, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        investment1.ModelPortfolioPercentage.Should().Be(35m);
        investment2.ModelPortfolioPercentage.Should().Be(30m);
        strategy.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void UpdateInvestmentsModelPercentages_Should_ReturnFailure_WhenInvestmentNotFound()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 10000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);
        
        string nonExistentInvestmentId = $"i_{Guid.NewGuid()}";
        var newPercentages = new Dictionary<string, decimal>
        {
            { nonExistentInvestmentId, 35m }
        };

        // Act
        Result result = strategy.UpdateInvestmentsModelPercentages(newPercentages, updatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvestmentStrategyErrors.InvestmentNotFound(nonExistentInvestmentId));
    }

    [Fact]
    public void UpdateInvestmentsModelPercentages_Should_ReturnFailure_WhenTotalPercentageExceeds100()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 10000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime investmentCreatedAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);
        Result<Investment> addResult1 = strategy.AddInvestment("Apple Inc.", investmentCreatedAtUtc);
        Result<Investment> addResult2 = strategy.AddInvestment("Microsoft", investmentCreatedAtUtc);
        addResult1.IsSuccess.Should().BeTrue();
        addResult2.IsSuccess.Should().BeTrue();
        Investment investment1 = addResult1.Value;
        Investment investment2 = addResult2.Value;

        var newPercentages = new Dictionary<string, decimal>
        {
            { investment1.Id, 60m },
            { investment2.Id, 50m } // Total: 110%
        };

        // Act
        Result result = strategy.UpdateInvestmentsModelPercentages(newPercentages, updatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvestmentStrategyErrors.ModelPortfolioPercentageExceeds100());
    }

    [Fact]
    public void UpdateInvestmentCurrentAmounts_Should_UpdateMultipleInvestmentAmounts()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 10000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime investmentCreatedAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);
        Result<Investment> addResult1 = strategy.AddInvestment("Apple Inc.", investmentCreatedAtUtc);
        Result<Investment> addResult2 = strategy.AddInvestment("Microsoft", investmentCreatedAtUtc);
        addResult1.IsSuccess.Should().BeTrue();
        addResult2.IsSuccess.Should().BeTrue();
        Investment investment1 = addResult1.Value;
        Investment investment2 = addResult2.Value;

        var newAmounts = new Dictionary<string, decimal>
        {
            { investment1.Id, 15000m },
            { investment2.Id, 12000m }
        };

        // Act
        strategy.UpdateInvestmentCurrentAmounts(newAmounts, updatedAtUtc);

        // Assert
        investment1.CurrentAmount.Should().Be(15000m);
        investment2.CurrentAmount.Should().Be(12000m);
        strategy.TotalCurrentAmount.Should().Be(27000m);
        strategy.TotalAmount.Should().Be(37000m); // 27000 + 10000 uninvested
        strategy.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void UpdateInvestmentCurrentAmounts_Should_PublishInvestmentStrategyCurrentAmountsUpdatedDomainEvent()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 10000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime investmentCreatedAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);
        Result<Investment> addResult = strategy.AddInvestment("Apple Inc.", investmentCreatedAtUtc);
        addResult.IsSuccess.Should().BeTrue();
        Investment investment = addResult.Value;

        var newAmounts = new Dictionary<string, decimal>
        {
            { investment.Id, 15000m }
        };

        // Act
        strategy.UpdateInvestmentCurrentAmounts(newAmounts, updatedAtUtc);

        // Assert
        InvestmentStrategyCurrentAmountsUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<InvestmentStrategyCurrentAmountsUpdatedDomainEvent>(strategy);
        domainEvent.StrategyId.Should().Be(strategy.Id);
        domainEvent.TotalCurrentAmount.Should().Be(15000m);
        domainEvent.TotalAmount.Should().Be(25000m); // 15000 + 10000 uninvested
        domainEvent.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void TotalInvestedAmount_Should_CalculateCorrectly_WithMultipleInvestments()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 10000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime investmentCreatedAtUtc = Faker.Date.PastOffset().UtcDateTime;

        string currencyId = $"c_{Guid.NewGuid()}";
        DateTime purchaseCreatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);
        Result<Investment> addResult1 = strategy.AddInvestment("Apple Inc.", investmentCreatedAtUtc);
        Result<Investment> addResult2 = strategy.AddInvestment("Microsoft", investmentCreatedAtUtc);
        addResult1.IsSuccess.Should().BeTrue();
        addResult2.IsSuccess.Should().BeTrue();
        Investment investment1 = addResult1.Value;
        Investment investment2 = addResult2.Value;

        // Act - Add purchase records to investments
        investment1.AddPurchaseRecord(Faker.Date.PastOffset().UtcDateTime, 100m, 150m, currencyId, purchaseCreatedAtUtc).IsSuccess.Should().BeTrue(); // 15,000
        investment2.AddPurchaseRecord(Faker.Date.PastOffset().UtcDateTime, 80m, 100m, currencyId, purchaseCreatedAtUtc).IsSuccess.Should().BeTrue();  // 8,000

        // Assert
        strategy.TotalInvestedAmount.Should().Be(23000m); // 15,000 + 8,000
    }

    [Fact]
    public void TotalCurrentAmount_Should_CalculateCorrectly_WithMultipleInvestments()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 10000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime investmentCreatedAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);
        Result<Investment> addResult1 = strategy.AddInvestment("Apple Inc.", investmentCreatedAtUtc);
        Result<Investment> addResult2 = strategy.AddInvestment("Microsoft", investmentCreatedAtUtc);
        addResult1.IsSuccess.Should().BeTrue();
        addResult2.IsSuccess.Should().BeTrue();
        Investment investment1 = addResult1.Value;
        Investment investment2 = addResult2.Value;

        // Act - Set current amounts
        investment1.UpdateCurrentAmount(18000m, updatedAtUtc);
        investment2.UpdateCurrentAmount(9500m, updatedAtUtc);

        // Assert
        strategy.TotalCurrentAmount.Should().Be(27500m); // 18,000 + 9,500
        strategy.TotalAmount.Should().Be(37500m); // 27,500 + 10,000 uninvested
    }

    [Fact]
    public void TotalAmount_Should_IncludeUninvestedAmount()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 15000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime investmentCreatedAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);
        Result<Investment> addResult = strategy.AddInvestment("Apple Inc.", investmentCreatedAtUtc);
        addResult.IsSuccess.Should().BeTrue();
        Investment investment = addResult.Value;

        // Act
        investment.UpdateCurrentAmount(20000m, updatedAtUtc);

        // Assert
        strategy.TotalCurrentAmount.Should().Be(20000m);
        strategy.UninvestedAmount.Should().Be(15000m);
        strategy.TotalAmount.Should().Be(35000m); // 20,000 invested + 15,000 uninvested
    }

    [Fact]
    public void RecalculateRealPortfolioPercentages_Should_CalculatePercentagesCorrectly()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 10000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime investmentCreatedAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);
        Result<Investment> addResult1 = strategy.AddInvestment("Apple Inc.", investmentCreatedAtUtc);    // Model: 30%
        Result<Investment> addResult2 = strategy.AddInvestment("Microsoft", investmentCreatedAtUtc);     // Model: 25%
        addResult1.IsSuccess.Should().BeTrue();
        addResult2.IsSuccess.Should().BeTrue();
        Investment investment1 = addResult1.Value;
        Investment investment2 = addResult2.Value;

        var newAmounts = new Dictionary<string, decimal>
        {
            { investment1.Id, 18000m }, // Current: 18,000
            { investment2.Id, 12000m }  // Current: 12,000
        };

        // Act
        strategy.UpdateInvestmentCurrentAmounts(newAmounts, updatedAtUtc);
        // Total current amount: 30,000 (18,000 + 12,000)

        // Assert
        investment1.RealPortfolioPercentage.Should().Be(60m); // 18,000 / 30,000 * 100 = 60%
        investment2.RealPortfolioPercentage.Should().Be(40m); // 12,000 / 30,000 * 100 = 40%
    }

    [Fact]
    public void RecalculateRealPortfolioPercentages_Should_SetZeroPercentages_WhenTotalCurrentAmountIsZero()
    {
        // Arrange
        string financialGoalId = $"fg_{Guid.NewGuid()}";
        string investmentStrategyTypeId = $"ist_{Guid.NewGuid()}";
        decimal uninvestedAmount = 10000m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime investmentCreatedAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var strategy = InvestmentStrategy.Create(financialGoalId, investmentStrategyTypeId, uninvestedAmount, createdAtUtc);
        Result<Investment> addResult1 = strategy.AddInvestment("Apple Inc.", investmentCreatedAtUtc);
        Result<Investment> addResult2 = strategy.AddInvestment("Microsoft", investmentCreatedAtUtc);
        addResult1.IsSuccess.Should().BeTrue();
        addResult2.IsSuccess.Should().BeTrue();
        Investment investment1 = addResult1.Value;
        Investment investment2 = addResult2.Value;

        var zeroAmounts = new Dictionary<string, decimal>
        {
            { investment1.Id, 0m },
            { investment2.Id, 0m }
        };

        // Act
        strategy.UpdateInvestmentCurrentAmounts(zeroAmounts, updatedAtUtc);

        // Assert
        investment1.RealPortfolioPercentage.Should().Be(0m);
        investment2.RealPortfolioPercentage.Should().Be(0m);
    }
}
