using FluentAssertions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.CreateSettlementPeriod;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.GetSettlementPeriods;
using ViaRiceco.Modules.Accounting.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Accounting.IntegrationTests.SettlementPeriods;

public sealed class GetSettlementPeriodsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnEmptyList_WhenNoSettlementPeriodsExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        var query = new GetSettlementPeriodsQuery(null, null, 1, 10, null, null);

        // Act
        Result<GetSettlementPeriodsQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Should_ReturnSettlementPeriods_WhenSettlementPeriodsExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        // Create test data
        await Sender.Send(new CreateSettlementPeriodCommand(3, 2024));
        await Sender.Send(new CreateSettlementPeriodCommand(4, 2024));
        
        var query = new GetSettlementPeriodsQuery(null, null, 1, 10, null, null);

        // Act
        Result<GetSettlementPeriodsQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        
        var periods = result.Value.Items.ToList();
        periods.Should().Contain(sp => sp.Month == 3 && sp.Year == 2024);
        periods.Should().Contain(sp => sp.Month == 4 && sp.Year == 2024);
    }

    [Fact]
    public async Task Should_ReturnFilteredSettlementPeriods_WhenSearchIsProvided()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        // Create test data
        await Sender.Send(new CreateSettlementPeriodCommand(3, 2024));
        await Sender.Send(new CreateSettlementPeriodCommand(4, 2025));
        
        var query = new GetSettlementPeriodsQuery("2024", null, 1, 10, null, null);

        // Act
        Result<GetSettlementPeriodsQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().Year.Should().Be(2024);
    }

    [Fact]
    public async Task Should_ReturnPaginatedResults_WhenPageSizeIsProvided()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        // Create test data
        for (int month = 1; month <= 3; month++)
        {
            await Sender.Send(new CreateSettlementPeriodCommand(month, 2024));
        }
        
        var query = new GetSettlementPeriodsQuery(null, null, 1, 2, null, null);

        // Act
        Result<GetSettlementPeriodsQueryResponse> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(3);
    }
}
