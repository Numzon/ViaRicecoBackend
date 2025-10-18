using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Budgets.Application.MonthlyBudgetExpenses.BulkSetExpenseValues;
using ViaRiceco.Modules.Budgets.Presentation.Enumerations;

namespace ViaRiceco.Modules.Budgets.Presentation.MonthlyBudgetExpenses;

internal sealed class BulkSetExpenseValuesEndpoint(ISender sender)
    : Ep.Req<BulkSetExpenseValuesEndpoint.Request>.Res<Result>
{
    [UsedImplicitly]
    internal sealed class Request
    {
        public string MonthlyBudgetId { get; init; } = string.Empty;
        public IReadOnlyCollection<ExpenseValueUpdateRequest> ExpenseValueUpdates { get; init; } = [];
    }

    [UsedImplicitly]
    internal sealed class ExpenseValueUpdateRequest
    {
        public string MonthlyBudgetExpenseId { get; init; } = string.Empty;
        public decimal? Value { get; init; }
    }

    public override void Configure()
    {
        Put("/budgets/monthly-budgets/{monthlyBudgetId}/expenses/values");
        AllowAnonymous();
        Description(d =>
        {
            d.WithName(nameof(BulkSetExpenseValuesEndpoint));
        });
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.MonthlyBudgetsExpenses)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var expenseValueUpdates = req.ExpenseValueUpdates
            .Select(update => new ExpenseValueUpdateDto(update.MonthlyBudgetExpenseId, update.Value))
            .ToList();

        var command = new BulkSetExpenseValuesCommand(req.MonthlyBudgetId, expenseValueUpdates);
        Result result = await sender.Send(command, ct);

        await Send.ResultAsync(result.Match(Results.NoContent, ApiResults.Problem));
    }
}
    
