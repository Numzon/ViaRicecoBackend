using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

public static class InvestmentStrategyErrors
{
    public static Error InvestmentNotFound(string investmentId) => Error.NotFound(
        "InvestmentStrategy.InvestmentNotFound", 
        $"Investment with Id '{investmentId}' was not found in this strategy.");
    
    public static Error ModelPortfolioPercentageExceeds100() => Error.Validation(
        "InvestmentStrategy.ModelPortfolioPercentageExceeds100", 
        "Total model portfolio percentage cannot exceed 100%.");

    public static Error NotFound(string strategyId) => Error.NotFound(
        "InvestmentStrategy.NotFound", 
        $"Investment strategy with Id '{strategyId}' was not found.");

    public static Error InvalidUninvestedAmount() => Error.Validation(
        "InvestmentStrategy.InvalidUninvestedAmount",
        "Uninvested amount must be greater than or equal to zero.");

    public static Error InvalidSortParameter(string? sort) => Error.Validation(
        "InvestmentStrategy.InvalidSortParameter", 
        $"Invalid sort parameter: '{sort}'");
    
    public static Error FinancialGoalMustBeRoot(string financialGoalId) => Error.Validation(
        "InvestmentStrategy.FinancialGoalMustBeRoot",
        $"Investment strategy can only be created for root financial goals. The financial goal '{financialGoalId}' has a parent and cannot be used for investment strategies.");
}
