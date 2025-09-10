using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.Investments;

public sealed class InvestmentCurrentAmountUpdatedDomainEvent(
    string investmentId, 
    decimal currentAmount, 
    decimal currentInvestedDifference, 
    DateTime updatedAtUtc) : DomainEvent
{
    public string InvestmentId { get; init; } = investmentId;
    public decimal CurrentAmount { get; init; } = currentAmount;
    public decimal CurrentInvestedDifference { get; init; } = currentInvestedDifference;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
