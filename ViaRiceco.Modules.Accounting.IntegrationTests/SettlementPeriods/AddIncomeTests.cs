using FluentAssertions;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.Incomes.AddIncome;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.CreateSettlementPeriod;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models;
using ViaRiceco.Modules.Accounting.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Accounting.IntegrationTests.SettlementPeriods;

public sealed class AddIncomeTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_AddIncome_WhenCommandIsValid()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        Result<SettlementPeriodDto> createSettlementPeriodResult = await Sender.Send(new CreateSettlementPeriodCommand(3, 2024));
        string settlementPeriodId = createSettlementPeriodResult.Value.Id;
        
        var command = new AddIncomeCommand(settlementPeriodId, 1000.00m);

        // Act
        Result<IncomeDto> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Value.Should().Be(1000.00m);
        result.Value.Id.Should().StartWith("i_");
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenSettlementPeriodDoesNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        var command = new AddIncomeCommand("sp_nonexistent", 1000.00m);

        // Act
        Result<IncomeDto> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenValueIsZeroOrNegative()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        Result<SettlementPeriodDto> createSettlementPeriodResult = await Sender.Send(new CreateSettlementPeriodCommand(3, 2024));
        string settlementPeriodId = createSettlementPeriodResult.Value.Id;
        
        var command = new AddIncomeCommand(settlementPeriodId, 0);

        // Act
        Result<IncomeDto> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenSettlementPeriodIdIsEmpty()
    {
        // Arrange
        await CleanDatabaseAsync();
        var command = new AddIncomeCommand(string.Empty, 1000.00m);

        // Act
        Result<IncomeDto> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
}
