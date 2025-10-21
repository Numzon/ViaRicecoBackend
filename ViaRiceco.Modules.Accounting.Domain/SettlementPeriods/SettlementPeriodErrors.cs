using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

public static class SettlementPeriodErrors
{
    public static Error NotFound(string settlementPeriodId) =>
        Error.NotFound("SettlementPeriod.NotFound", $"The settlement period with the identifier {settlementPeriodId} was not found");

    public static Error AlreadyExists(int month, int year) =>
        Error.Conflict("SettlementPeriod.AlreadyExists", $"A settlement period for {month:D2}/{year} already exists");

    public static Error InvalidMonth(int month) =>
        Error.Validation("SettlementPeriod.InvalidMonth", $"Month must be between 1 and 12, but was {month}");

    public static Error InvalidYear(int year) =>
        Error.Validation("SettlementPeriod.InvalidYear", $"Year must be between 1900 and 2100, but was {year}");

    public static Error CannotDeleteWithData() =>
        Error.Conflict("SettlementPeriod.CannotDeleteWithData", "Cannot delete settlement period that contains incomes or taxes");

    public static Error TaxTypeAlreadyExists(string taxTypeId) =>
        Error.Conflict("SettlementPeriod.TaxTypeAlreadyExists", $"A tax of type {taxTypeId} already exists in this settlement period. Only one tax per type is allowed per period");

    public static Error CannotModifyFinalizedPeriod() =>
        Error.Conflict("SettlementPeriod.CannotModifyFinalizedPeriod", "Cannot modify a finalized settlement period. Set it as draft first to make changes");

    public static Error CannotSetAsNonRecentDraft(int month, int year) =>
        Error.Conflict("SettlementPeriod.CannotSetAsNonRecentDraft", $"Cannot set settlement period {month:D2}/{year} as draft. Only the most recent settlement period can be set as draft");

    public static Error DraftPeriodExists() =>
        Error.Conflict("SettlementPeriod.DraftPeriodExists", "Cannot create a new settlement period while there is an existing draft period. Finalize the draft period first");
}
