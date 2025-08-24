using FluentAssertions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Domain.Incomes;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;
using ViaRiceco.Modules.Accounting.Domain.Taxes;
using ViaRiceco.Modules.Accounting.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Accounting.UnitTests.SettlementPeriods;

public sealed class SettlementPeriodTests : BaseTest
{
    [Fact]
    public void Create_Should_CreateSettlementPeriodWithValidData()
    {
        // Arrange
        int month = Faker.Random.Int(1, 12);
        int year = Faker.Random.Int(2000, 2030);
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var period = SettlementPeriod.Create(month, year, createdAtUtc);

        // Assert
        period.Should().NotBeNull();
        period.Id.Should().StartWith("sp_");
        period.Month.Should().Be(month);
        period.Year.Should().Be(year);
        period.CreatedAtUtc.Should().Be(createdAtUtc);
        period.UpdatedAtUtc.Should().BeNull();
        period.Incomes.Should().BeEmpty();
        period.Taxes.Should().BeEmpty();
        period.TotalIncome.Should().Be(0);
        period.TotalTaxes.Should().Be(0);
        period.NetAmount.Should().Be(0);
    }

    [Fact]
    public void Create_Should_GenerateUniqueIds()
    {
        // Arrange
        int month = Faker.Random.Int(1, 12);
        int year = Faker.Random.Int(2000, 2030);
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var period1 = SettlementPeriod.Create(month, year, createdAtUtc);
        var period2 = SettlementPeriod.Create(month, year, createdAtUtc);

        // Assert
        period1.Id.Should().NotBe(period2.Id);
        period1.Id.Should().StartWith("sp_");
        period2.Id.Should().StartWith("sp_");
    }

    [Fact]
    public void Create_Should_PublishSettlementPeriodCreatedDomainEvent()
    {
        // Arrange
        int month = Faker.Random.Int(1, 12);
        int year = Faker.Random.Int(2000, 2030);
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var period = SettlementPeriod.Create(month, year, createdAtUtc);

        // Assert
        SettlementPeriodCreatedDomainEvent domainEvent = AssertDomainEventWasPublished<SettlementPeriodCreatedDomainEvent>(period);
        domainEvent.SettlementPeriodId.Should().Be(period.Id);
        domainEvent.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void Create_Should_ThrowArgumentException_WhenMonthIsInvalid(int invalidMonth)
    {
        // Arrange
        int year = Faker.Random.Int(2000, 2030);
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act & Assert
        Action action = () => SettlementPeriod.Create(invalidMonth, year, createdAtUtc);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Month must be between 1 and 12*")
            .And.ParamName.Should().Be("month");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void Create_Should_AcceptValidMonths(int validMonth)
    {
        // Arrange
        int year = Faker.Random.Int(2000, 2030);
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var period = SettlementPeriod.Create(validMonth, year, createdAtUtc);

        // Assert
        period.Should().NotBeNull();
        period.Month.Should().Be(validMonth);
    }

    [Theory]
    [InlineData(1800)]
    [InlineData(2100)]
    [InlineData(-100)]
    public void Create_Should_AcceptAnyYear(int year)
    {
        // Note: Year validation was removed as requested
        
        // Arrange
        int month = Faker.Random.Int(1, 12);
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var period = SettlementPeriod.Create(month, year, createdAtUtc);

        // Assert
        period.Should().NotBeNull();
        period.Year.Should().Be(year);
    }

    [Fact]
    public void AddIncome_Should_AddIncomeToCollection()
    {
        // Arrange
        SettlementPeriod period = CreateSettlementPeriod();
        decimal incomeValue = Faker.Random.Decimal(1000, 10000);
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        period.AddIncome(incomeValue, createdAtUtc);

        // Assert
        period.Incomes.Should().HaveCount(1);
        Income income = period.Incomes.First();
        income.Value.Should().Be(incomeValue);
        income.SettlementPeriodId.Should().Be(period.Id);
        period.TotalIncome.Should().Be(incomeValue);
        period.NetAmount.Should().Be(incomeValue);
    }

    [Fact]
    public void AddIncome_Should_PublishBothIncomeAddedAndNetAmountRecalculatedEvents()
    {
        // Arrange
        SettlementPeriod period = CreateSettlementPeriod();
        decimal incomeValue = Faker.Random.Decimal(1000, 10000);
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        period.AddIncome(incomeValue, createdAtUtc);

        // Assert
        IncomeAddedToSettlementPeriodDomainEvent incomeAddedEvent = AssertDomainEventWasPublished<IncomeAddedToSettlementPeriodDomainEvent>(period);
        incomeAddedEvent.SettlementPeriodId.Should().Be(period.Id);
        incomeAddedEvent.Value.Should().Be(incomeValue);
        incomeAddedEvent.CreatedAtUtc.Should().Be(createdAtUtc);

        SettlementPeriodNetAmountRecalculatedDomainEvent netAmountEvent = AssertDomainEventWasPublished<SettlementPeriodNetAmountRecalculatedDomainEvent>(period);
        netAmountEvent.SettlementPeriodId.Should().Be(period.Id);
        netAmountEvent.TotalIncome.Should().Be(incomeValue);
        netAmountEvent.TotalTaxes.Should().Be(0);
        netAmountEvent.NetAmount.Should().Be(incomeValue);
        netAmountEvent.RecalculatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void AddTax_Should_AddTaxToCollection_WhenTaxTypeIsNotDuplicate()
    {
        // Arrange
        SettlementPeriod period = CreateSettlementPeriod();
        decimal taxValue = Faker.Random.Decimal(100, 1000);
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result result = period.AddTax(taxValue, taxTypeId, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        period.Taxes.Should().HaveCount(1);
        Tax tax = period.Taxes.First();
        tax.Value.Should().Be(taxValue);
        tax.TaxTypeId.Should().Be(taxTypeId);
        tax.SettlementPeriodId.Should().Be(period.Id);
        period.TotalTaxes.Should().Be(taxValue);
        period.NetAmount.Should().Be(-taxValue);
    }

    [Fact]
    public void AddTax_Should_ReturnFailure_WhenTaxTypeAlreadyExists()
    {
        // Arrange
        SettlementPeriod period = CreateSettlementPeriod();
        decimal taxValue1 = Faker.Random.Decimal(100, 500);
        decimal taxValue2 = Faker.Random.Decimal(600, 1000);
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        period.AddTax(taxValue1, taxTypeId, createdAtUtc);

        // Act
        Result result = period.AddTax(taxValue2, taxTypeId, createdAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SettlementPeriodErrors.TaxTypeAlreadyExists(taxTypeId));
        period.Taxes.Should().HaveCount(1); // Should still only have the first tax
        period.TotalTaxes.Should().Be(taxValue1); // Should not include the second tax
    }

    [Fact]
    public void AddTax_Should_PublishBothTaxAddedAndNetAmountRecalculatedEvents()
    {
        // Arrange
        SettlementPeriod period = CreateSettlementPeriod();
        decimal taxValue = Faker.Random.Decimal(100, 1000);
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result result = period.AddTax(taxValue, taxTypeId, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        TaxAddedToSettlementPeriodDomainEvent taxAddedEvent = AssertDomainEventWasPublished<TaxAddedToSettlementPeriodDomainEvent>(period);
        taxAddedEvent.SettlementPeriodId.Should().Be(period.Id);
        taxAddedEvent.Value.Should().Be(taxValue);
        taxAddedEvent.TaxTypeId.Should().Be(taxTypeId);
        taxAddedEvent.CreatedAtUtc.Should().Be(createdAtUtc);

        SettlementPeriodNetAmountRecalculatedDomainEvent netAmountEvent = AssertDomainEventWasPublished<SettlementPeriodNetAmountRecalculatedDomainEvent>(period);
        netAmountEvent.TotalIncome.Should().Be(0);
        netAmountEvent.TotalTaxes.Should().Be(taxValue);
        netAmountEvent.NetAmount.Should().Be(-taxValue);
    }

    [Fact]
    public void RemoveIncome_Should_RemoveIncomeAndUpdateTotals()
    {
        // Arrange
        SettlementPeriod period = CreateSettlementPeriod();
        decimal incomeValue = Faker.Random.Decimal(1000, 10000);
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime removedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        period.AddIncome(incomeValue, createdAtUtc);
        string incomeId = period.Incomes.First().Id;

        // Act
        Result result = period.RemoveIncome(incomeId, removedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        period.Incomes.Should().BeEmpty();
        period.TotalIncome.Should().Be(0);
        period.UpdatedAtUtc.Should().Be(removedAtUtc);
    }

    [Fact]
    public void RemoveIncome_Should_ReturnFailure_WhenIncomeNotFound()
    {
        // Arrange
        SettlementPeriod period = CreateSettlementPeriod();
        string nonExistentIncomeId = $"i_{Guid.NewGuid()}";
        DateTime removedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result result = period.RemoveIncome(nonExistentIncomeId, removedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(IncomeErrors.NotFound(nonExistentIncomeId));
    }

    [Fact]
    public void RemoveTax_Should_RemoveTaxAndUpdateTotals()
    {
        // Arrange
        SettlementPeriod period = CreateSettlementPeriod();
        decimal taxValue = Faker.Random.Decimal(100, 1000);
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime removedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        period.AddTax(taxValue, taxTypeId, createdAtUtc);
        string taxId = period.Taxes.First().Id;

        // Act
        Result result = period.RemoveTax(taxId, removedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        period.Taxes.Should().BeEmpty();
        period.TotalTaxes.Should().Be(0);
        period.UpdatedAtUtc.Should().Be(removedAtUtc);
    }

    [Fact]
    public void RemoveTax_Should_ReturnFailure_WhenTaxNotFound()
    {
        // Arrange
        SettlementPeriod period = CreateSettlementPeriod();
        string nonExistentTaxId = $"t_{Guid.NewGuid()}";
        DateTime removedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result result = period.RemoveTax(nonExistentTaxId, removedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaxErrors.NotFound(nonExistentTaxId));
    }

    [Fact]
    public void UpdateIncome_Should_UpdateIncomeValueAndPublishEvents()
    {
        // Arrange
        SettlementPeriod period = CreateSettlementPeriod();
        decimal originalValue = Faker.Random.Decimal(1000, 5000);
        decimal newValue = Faker.Random.Decimal(5001, 10000);
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        period.AddIncome(originalValue, createdAtUtc);
        string incomeId = period.Incomes.First().Id;

        // Clear existing events to focus on update events
        period.ClearDomainEvents();

        // Act
        Result result = period.UpdateIncome(incomeId, newValue, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        period.Incomes.First().Value.Should().Be(newValue);
        period.TotalIncome.Should().Be(newValue);
        period.UpdatedAtUtc.Should().Be(updatedAtUtc);

        SettlementPeriodNetAmountRecalculatedDomainEvent netAmountEvent = AssertDomainEventWasPublished<SettlementPeriodNetAmountRecalculatedDomainEvent>(period);
        netAmountEvent.TotalIncome.Should().Be(newValue);
    }

    [Fact]
    public void UpdateTax_Should_UpdateTaxValueAndPublishEvents()
    {
        // Arrange
        SettlementPeriod period = CreateSettlementPeriod();
        decimal originalValue = Faker.Random.Decimal(100, 500);
        decimal newValue = Faker.Random.Decimal(501, 1000);
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        period.AddTax(originalValue, taxTypeId, createdAtUtc);
        string taxId = period.Taxes.First().Id;

        // Clear existing events to focus on update events
        period.ClearDomainEvents();

        // Act
        Result result = period.UpdateTax(taxId, newValue, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        period.Taxes.First().Value.Should().Be(newValue);
        period.TotalTaxes.Should().Be(newValue);
        period.UpdatedAtUtc.Should().Be(updatedAtUtc);

        SettlementPeriodNetAmountRecalculatedDomainEvent netAmountEvent = AssertDomainEventWasPublished<SettlementPeriodNetAmountRecalculatedDomainEvent>(period);
        netAmountEvent.TotalTaxes.Should().Be(newValue);
    }

    [Fact]
    public void NetAmount_Should_CalculateCorrectly_WithMultipleIncomesAndTaxes()
    {
        // Arrange
        SettlementPeriod period = CreateSettlementPeriod();
        decimal income1 = 5000m;
        decimal income2 = 3000m;
        decimal tax1 = 750m;
        decimal tax2 = 450m;
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        period.AddIncome(income1, createdAtUtc);
        period.AddIncome(income2, createdAtUtc);
        period.AddTax(tax1, $"tt_{Guid.NewGuid()}", createdAtUtc);
        period.AddTax(tax2, $"tt_{Guid.NewGuid()}", createdAtUtc);

        // Assert
        period.TotalIncome.Should().Be(income1 + income2);
        period.TotalTaxes.Should().Be(tax1 + tax2);
        period.NetAmount.Should().Be(income1 + income2 - (tax1 + tax2));
    }

    [Fact]
    public void AddTax_Should_AllowMultipleTaxTypes()
    {
        // Arrange
        SettlementPeriod period = CreateSettlementPeriod();
        decimal taxValue1 = 500m;
        decimal taxValue2 = 750m;
        string taxType1 = $"tt_{Guid.NewGuid()}";
        string taxType2 = $"tt_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result result1 = period.AddTax(taxValue1, taxType1, createdAtUtc);
        Result result2 = period.AddTax(taxValue2, taxType2, createdAtUtc);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        period.Taxes.Should().HaveCount(2);
        period.TotalTaxes.Should().Be(taxValue1 + taxValue2);
    }

    [Fact]
    public void RemoveOperations_Should_PublishNetAmountRecalculatedEvents()
    {
        // Arrange
        SettlementPeriod period = CreateSettlementPeriod();
        decimal incomeValue = 5000m;
        decimal taxValue = 750m;
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime removedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        period.AddIncome(incomeValue, createdAtUtc);
        period.AddTax(taxValue, taxTypeId, createdAtUtc);
        string incomeId = period.Incomes.First().Id;

        // Clear existing events to focus on removal events
        period.ClearDomainEvents();

        // Act
        Result result = period.RemoveIncome(incomeId, removedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        SettlementPeriodNetAmountRecalculatedDomainEvent netAmountEvent = AssertDomainEventWasPublished<SettlementPeriodNetAmountRecalculatedDomainEvent>(period);
        netAmountEvent.TotalIncome.Should().Be(0);
        netAmountEvent.TotalTaxes.Should().Be(taxValue);
        netAmountEvent.NetAmount.Should().Be(-taxValue);
    }

    private static SettlementPeriod CreateSettlementPeriod()
    {
        return SettlementPeriod.Create(
            Faker.Random.Int(1, 12), 
            Faker.Random.Int(2000, 2030), 
            Faker.Date.RecentOffset().UtcDateTime
        );
    }
}
