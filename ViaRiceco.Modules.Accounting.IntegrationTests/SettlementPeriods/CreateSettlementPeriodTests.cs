using FluentAssertions;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.CreateSettlementPeriod;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models;
using ViaRiceco.Modules.Accounting.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Accounting.IntegrationTests.SettlementPeriods;

public sealed class CreateSettlementPeriodTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_CreateSettlementPeriod_WhenCommandIsValid()
    {
        // Arrange
        await CleanDatabaseAsync();
        var command = new CreateSettlementPeriodCommand(3, 2024);

        // Act
        Result<SettlementPeriodDto> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Month.Should().Be(3);
        result.Value.Year.Should().Be(2024);
        result.Value.TotalIncome.Should().Be(0);
        result.Value.TotalTaxes.Should().Be(0);
        result.Value.NetAmount.Should().Be(0);
        result.Value.Incomes.Should().BeEmpty();
        result.Value.Taxes.Should().BeEmpty();
        result.Value.Id.Should().StartWith("sp_");
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenMonthIsInvalid()
    {
        // Arrange
        await CleanDatabaseAsync();
        var command = new CreateSettlementPeriodCommand(13, 2024);

        // Act
        Result<SettlementPeriodDto> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenYearIsInvalid()
    {
        // Arrange
        await CleanDatabaseAsync();
        var command = new CreateSettlementPeriodCommand(3, 1800);

        // Act
        Result<SettlementPeriodDto> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenSettlementPeriodAlreadyExists()
    {
        // Arrange
        await CleanDatabaseAsync();
        var firstCommand = new CreateSettlementPeriodCommand(3, 2024);
        await Sender.Send(firstCommand);
        
        var duplicateCommand = new CreateSettlementPeriodCommand(3, 2024);

        // Act
        Result<SettlementPeriodDto> result = await Sender.Send(duplicateCommand);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }
}
