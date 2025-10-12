using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Domain.Incomes;
using ViaRiceco.Modules.Accounting.Domain.Taxes;

namespace ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

public sealed class SettlementPeriod : Entity
{
    private readonly List<Income> _incomes = [];
    private readonly List<Tax> _taxes = [];

    private SettlementPeriod()
    {
    }

    public int Month { get; private set; }
    public int Year { get; private set; }
    public bool IsDraft { get; private set; }
    
    public IReadOnlyCollection<Income> Incomes => _incomes.AsReadOnly();
    public IReadOnlyCollection<Tax> Taxes => _taxes.AsReadOnly();

    public decimal TotalIncome => _incomes.Sum(i => i.Value);
    public decimal TotalTaxes => _taxes.Sum(t => t.Value);
    public decimal NetAmount => TotalIncome - TotalTaxes;

    public static SettlementPeriod Create(int month, int year, DateTime createdAtUtc)
    {
        if (month < 1 || month > 12)
        {
            throw new ArgumentException("Month must be between 1 and 12", nameof(month));
        }

        var period = new SettlementPeriod
        {
            Id = $"sp_{Guid.NewGuid()}",
            Month = month,
            Year = year,
            IsDraft = true, // New settlement periods start as drafts
            CreatedAtUtc = createdAtUtc
        };

        period.Raise(new SettlementPeriodCreatedDomainEvent(period.Id, createdAtUtc));

        return period;
    }

    public Income AddIncome(decimal value, DateTime createdAtUtc)
    {
        if (!IsDraft)
        {
            throw new InvalidOperationException("Cannot modify finalized settlement period");
        }

        var income = Income.Create(value, Id, createdAtUtc);
        _incomes.Add(income);

        Raise(new IncomeAddedToSettlementPeriodDomainEvent(Id, income.Id, value, createdAtUtc));
        RaiseNetAmountRecalculatedEvent(createdAtUtc);
        
        return income;
    }

    public Result AddTax(decimal value, string taxTypeId, DateTime createdAtUtc)
    {
        if (!IsDraft)
        {
            return Result.Failure(SettlementPeriodErrors.CannotModifyFinalizedPeriod());
        }

        if (SettlementPeriodSpecification.TaxTypeAlreadyExists(this, taxTypeId))
        {
            return Result.Failure(SettlementPeriodErrors.TaxTypeAlreadyExists(taxTypeId));
        }

        var tax = Tax.Create(value, taxTypeId, Id, createdAtUtc);
        _taxes.Add(tax);

        Raise(new TaxAddedToSettlementPeriodDomainEvent(Id, tax.Id, value, taxTypeId, createdAtUtc));
        RaiseNetAmountRecalculatedEvent(createdAtUtc);

        return Result.Success(tax);
    }

    public Result RemoveIncome(string incomeId, DateTime updatedAtUtc)
    {
        if (!IsDraft)
        {
            return Result.Failure(SettlementPeriodErrors.CannotModifyFinalizedPeriod());
        }

        Income income = _incomes.Find(i => i.Id == incomeId);
        if (income == null)
        {
            return Result.Failure(IncomeErrors.NotFound(incomeId));
        }

        _incomes.Remove(income);
        UpdatedAtUtc = updatedAtUtc;

        Raise(new IncomeRemovedFromSettlementPeriodDomainEvent(Id, incomeId, updatedAtUtc));
        RaiseNetAmountRecalculatedEvent(updatedAtUtc);

        return Result.Success();
    }

    public Result RemoveTax(string taxId, DateTime updatedAtUtc)
    {
        if (!IsDraft)
        {
            return Result.Failure(SettlementPeriodErrors.CannotModifyFinalizedPeriod());
        }

        Tax tax = _taxes.Find(t => t.Id == taxId);
        if (tax == null)
        {
            return Result.Failure(TaxErrors.NotFound(taxId));
        }

        _taxes.Remove(tax);
        UpdatedAtUtc = updatedAtUtc;

        Raise(new TaxRemovedFromSettlementPeriodDomainEvent(Id, taxId, updatedAtUtc));
        RaiseNetAmountRecalculatedEvent(updatedAtUtc);

        return Result.Success();
    }

    public Result<Income> UpdateIncome(string incomeId, decimal value, DateTime updatedAtUtc)
    {
        if (!IsDraft)
        {
            return Result.Failure<Income>(SettlementPeriodErrors.CannotModifyFinalizedPeriod());
        }

        Income income = _incomes.Find(i => i.Id == incomeId);
        if (income == null)
        {
            return Result.Failure<Income>(IncomeErrors.NotFound(incomeId));
        }

        income.Update(value, updatedAtUtc);
        UpdatedAtUtc = updatedAtUtc;

        RaiseNetAmountRecalculatedEvent(updatedAtUtc);

        return Result.Success(income);
    }

    public Result<Tax> UpdateTax(string taxId, decimal value, DateTime updatedAtUtc)
    {
        if (!IsDraft)
        {
            return Result.Failure<Tax>(SettlementPeriodErrors.CannotModifyFinalizedPeriod());
        }

        Tax tax = _taxes.Find(t => t.Id == taxId);
        if (tax == null)
        {
            return Result.Failure<Tax>(TaxErrors.NotFound(taxId));
        }

        tax.Update(value, updatedAtUtc);
        UpdatedAtUtc = updatedAtUtc;

        RaiseNetAmountRecalculatedEvent(updatedAtUtc);

        return Result.Success(tax);
    }

    /// <summary>
    /// Finalizes the settlement period, preventing further modifications
    /// </summary>
    public Result Finalize(DateTime finalizedAtUtc)
    {
        if (!IsDraft)
        {
            return Result.Success(); // Already finalized
        }

        IsDraft = false;
        UpdatedAtUtc = finalizedAtUtc;

        Raise(new SettlementPeriodFinalizedDomainEvent(Id, finalizedAtUtc));

        return Result.Success();
    }

    /// <summary>
    /// Sets the settlement period back to draft status, allowing modifications
    /// This also triggers setting the monthly budget as draft
    /// </summary>
    public Result SetAsDraft(DateTime updatedAtUtc)
    {
        if (IsDraft)
        {
            return Result.Success(); // Already draft
        }

        IsDraft = true;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new SettlementPeriodSetAsDraftDomainEvent(Id, updatedAtUtc));

        return Result.Success();
    }

    /// <summary>
    /// Raises domain event for net amount recalculation after financial changes
    /// </summary>
    private void RaiseNetAmountRecalculatedEvent(DateTime eventTimeUtc)
    {
        Raise(new SettlementPeriodNetAmountRecalculatedDomainEvent(Id, TotalIncome, TotalTaxes, NetAmount, eventTimeUtc));
    }

    /// <summary>
    /// Prepares the settlement period for deletion by raising the appropriate domain event
    /// </summary>
    public void PrepareForDeletion(DateTime deletedAtUtc)
    {
        Raise(new SettlementPeriodDeletedDomainEvent(Id, Month, Year, deletedAtUtc));
    }
}
