using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

public static class InvestmentStrategyErrors
{
    public static Error InvestmentNotFound(string investmentId) => Error.NotFound(
        "InvestmentStrategy.InvestmentNotFound", 
        $"Investment with Id '{investmentId}' was not found in this strategy.");
    
    public static Error ModelPortfolioPercentageExceeds100(decimal totalPercentage) => Error.Validation(
        "InvestmentStrategy.ModelPortfolioPercentageExceeds100", 
        $"Total model portfolio percentage cannot exceed 100%. Current total: {totalPercentage}%");
}
