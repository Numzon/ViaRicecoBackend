using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.Incomes;

public static class IncomeErrors
{
    public static Error NotFound(string incomeId) =>
        Error.NotFound("Income.NotFound", $"The income with the identifier {incomeId} was not found");

    public static Error SettlementPeriodNotFound(string settlementPeriodId) =>
        Error.NotFound("Income.SettlementPeriodNotFound", $"The settlement period with identifier {settlementPeriodId} does not exist");
}
