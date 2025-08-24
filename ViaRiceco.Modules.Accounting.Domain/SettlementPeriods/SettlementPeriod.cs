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
            CreatedAtUtc = createdAtUtc
        };

        period.Raise(new SettlementPeriodCreatedDomainEvent(period.Id, createdAtUtc));

        return period;
    }

    public void AddIncome(decimal value, DateTime createdAtUtc)
    {
        var income = Income.Create(value, Id, createdAtUtc);
        _incomes.Add(income);

        Raise(new IncomeAddedToSettlementPeriodDomainEvent(Id, income.Id, value, createdAtUtc));
        RaiseNetAmountRecalculatedEvent(createdAtUtc);
    }

    public Result AddTax(decimal value, string taxTypeId, DateTime createdAtUtc)
    {
        // Business rule: Only one tax per tax type per settlement period
        if (_taxes.Any(t => t.TaxTypeId == taxTypeId))
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

    public Result UpdateIncome(string incomeId, decimal value, DateTime updatedAtUtc)
    {
        Income income = _incomes.Find(i => i.Id == incomeId);
        if (income == null)
        {
            return Result.Failure(IncomeErrors.NotFound(incomeId));
        }

        income.Update(value, updatedAtUtc);
        UpdatedAtUtc = updatedAtUtc;

        RaiseNetAmountRecalculatedEvent(updatedAtUtc);

        return Result.Success();
    }

    public Result UpdateTax(string taxId, decimal value, DateTime updatedAtUtc)
    {
        Tax tax = _taxes.Find(t => t.Id == taxId);
        if (tax == null)
        {
            return Result.Failure(TaxErrors.NotFound(taxId));
        }

        tax.Update(value, updatedAtUtc);
        UpdatedAtUtc = updatedAtUtc;

        RaiseNetAmountRecalculatedEvent(updatedAtUtc);

        return Result.Success();
    }

    /// <summary>
    /// Raises domain event for net amount recalculation after financial changes
    /// </summary>
    private void RaiseNetAmountRecalculatedEvent(DateTime eventTimeUtc)
    {
        Raise(new SettlementPeriodNetAmountRecalculatedDomainEvent(Id, TotalIncome, TotalTaxes, NetAmount, eventTimeUtc));
    }
}
