using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentTypes;

public static class InvestmentStrategyTypeErrors
{
    public static Error NotFound(string strategyTypeId) => Error.NotFound(
        "InvestmentStrategyType.NotFound", 
        $"Investment strategy type with Id '{strategyTypeId}' was not found.");
    
    public static Error DuplicateName(string name) => Error.Conflict(
        "InvestmentStrategyType.DuplicateName", 
        $"Investment strategy type with name '{name}' already exists.");
    
    public static Error InvalidName(string name) => Error.Validation(
        "InvestmentStrategyType.InvalidName", 
        $"Investment strategy type name '{name}' is invalid. Name cannot be empty.");

    public static Error InvalidSortParameter(string? sort) => Error.Validation(
        "InvestmentStrategyType.InvalidSortParameter", 
        $"Invalid sort parameter: '{sort}'");
}
