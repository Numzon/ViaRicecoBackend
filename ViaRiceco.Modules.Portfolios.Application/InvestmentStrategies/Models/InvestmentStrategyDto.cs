namespace ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.Models;

public sealed record InvestmentStrategyDto(
    string Id,
    string FinancialGoalId,
    string InvestmentStrategyTypeId,
    decimal UninvestedAmount,
    decimal TotalInvestedAmount,
    decimal TotalCurrentAmount,
    decimal TotalAmount,
    IReadOnlyCollection<InvestmentSummaryDto> Investments);

public sealed record InvestmentSummaryDto(
    string Id,
    string Name,
    decimal ModelPortfolioPercentage,
    decimal RealPortfolioPercentage,
    decimal InvestedAmount,
    decimal CurrentAmount);
