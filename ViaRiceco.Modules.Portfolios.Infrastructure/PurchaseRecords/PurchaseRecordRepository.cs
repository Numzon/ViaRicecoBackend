using Microsoft.EntityFrameworkCore;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;
using ViaRiceco.Modules.Portfolios.Infrastructure.Database;
using System.Linq.Dynamic.Core;
using ViaRiceco.Common.Domain.Parameters;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.PurchaseRecords;

internal sealed class PurchaseRecordRepository(PortfoliosDbContext context) : IPurchaseRecordRepository
{
    public Task<PurchaseRecord?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return context.PurchaseRecords.FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<PurchaseRecord>> GetByInvestmentAsync(string investmentId, CancellationToken cancellationToken = default)
    {
        return await context.PurchaseRecords
            .Where(pr => pr.InvestmentId == investmentId)
            .OrderBy(pr => pr.PurchaseDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PurchaseRecord>> GetByCurrencyAsync(string currencyId, CancellationToken cancellationToken = default)
    {
        return await context.PurchaseRecords
            .Where(pr => pr.CurrencyId == currencyId)
            .OrderBy(pr => pr.PurchaseDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PurchaseRecord>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await context.PurchaseRecords
            .Where(pr => pr.PurchaseDate >= fromDate && pr.PurchaseDate <= toDate)
            .OrderBy(pr => pr.PurchaseDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PurchaseRecord>> GetPageAsync(BaseQueryParameters queryParameters, DateRangeParameters dateRange, string? investmentId, string? currencyId, CancellationToken cancellationToken = default)
    {
        IQueryable<PurchaseRecord> query = context.PurchaseRecords.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(queryParameters.Search))
        {
            string cleanedSearch = queryParameters.Search.Trim().ToLowerInvariant();
            decimal? searchAmount = null;
            
            if (decimal.TryParse(cleanedSearch, System.Globalization.CultureInfo.InvariantCulture, out decimal parsedAmount))
            {
                searchAmount = parsedAmount;
            }
            
            query = query.Where(pr => 
                searchAmount != null && (pr.Amount == searchAmount || pr.PricePerUnit == searchAmount || pr.TotalPrice == searchAmount) ||
                EF.Functions.ILike(pr.InvestmentId, $"%{cleanedSearch}%") ||
                EF.Functions.ILike(pr.CurrencyId, $"%{cleanedSearch}%"));
        }

        if (!string.IsNullOrWhiteSpace(investmentId))
        {
            query = query.Where(pr => pr.InvestmentId == investmentId);
        }

        if (!string.IsNullOrWhiteSpace(currencyId))
        {
            query = query.Where(pr => pr.CurrencyId == currencyId);
        }

        if (dateRange.FromDate.HasValue)
        {
            query = query.Where(pr => pr.PurchaseDate >= dateRange.FromDate.Value);
        }

        if (dateRange.ToDate.HasValue)
        {
            query = query.Where(pr => pr.PurchaseDate <= dateRange.ToDate.Value);
        }

        return await query
            .OrderBy(queryParameters.OrderBy)
            .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
            .Take(queryParameters.PageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(BaseQueryParameters queryParameters, DateRangeParameters dateRange, string? investmentId, string? currencyId, CancellationToken cancellationToken = default)
    {
        IQueryable<PurchaseRecord> query = context.PurchaseRecords.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(queryParameters.Search))
        {
            string cleanedSearch = queryParameters.Search.Trim().ToLowerInvariant();
            decimal? searchAmount = null;
            
            if (decimal.TryParse(cleanedSearch, System.Globalization.CultureInfo.InvariantCulture, out decimal parsedAmount))
            {
                searchAmount = parsedAmount;
            }
            
            query = query.Where(pr => 
                searchAmount != null && (pr.Amount == searchAmount || pr.PricePerUnit == searchAmount || pr.TotalPrice == searchAmount) ||
                EF.Functions.ILike(pr.InvestmentId, $"%{cleanedSearch}%") ||
                EF.Functions.ILike(pr.CurrencyId, $"%{cleanedSearch}%"));
        }

        if (!string.IsNullOrWhiteSpace(investmentId))
        {
            query = query.Where(pr => pr.InvestmentId == investmentId);
        }

        if (!string.IsNullOrWhiteSpace(currencyId))
        {
            query = query.Where(pr => pr.CurrencyId == currencyId);
        }

        if (dateRange.FromDate.HasValue)
        {
            query = query.Where(pr => pr.PurchaseDate >= dateRange.FromDate.Value);
        }

        if (dateRange.ToDate.HasValue)
        {
            query = query.Where(pr => pr.PurchaseDate <= dateRange.ToDate.Value);
        }

        return query.CountAsync(cancellationToken);
    }

    public void Insert(PurchaseRecord purchaseRecord)
    {
        context.PurchaseRecords.Add(purchaseRecord);
    }

    public void Delete(PurchaseRecord purchaseRecord)
    {
        context.PurchaseRecords.Remove(purchaseRecord);
    }
}
