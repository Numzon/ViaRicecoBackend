using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Budgets.Application.MonthlyBudgetExpenses.RemoveMonthlyBudgetExpense;
using ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.Models;
using ViaRiceco.Modules.Budgets.Presentation.Enumerations;

namespace ViaRiceco.Modules.Budgets.Presentation.MonthlyBudgetExpenses;

internal sealed class RemoveMonthlyBudgetExpenseEndpoint(ISender sender)
    : Ep.Req<RemoveMonthlyBudgetExpenseEndpoint.Request>.Res<Result<MonthlyBudgetDto>>
{
    [UsedImplicitly]
    internal sealed class Request
    {
        public string MonthlyBudgetId { get; init; } = string.Empty;
        public string Id { get; init; } = string.Empty;
    }

    public override void Configure()
    {
        Delete("/budgets/monthly-budgets/{monthlyBudgetId}/expense/{id}");
        AllowAnonymous();
        Description(d =>
        {
            d.WithName(nameof(RemoveMonthlyBudgetExpenseEndpoint));
        });
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.MonthlyBudgetsExpenses)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new RemoveMonthlyBudgetExpenseCommand(req.MonthlyBudgetId, req.Id);
        Result<MonthlyBudgetDto> result = await sender.Send(command, ct);

        await Send.ResultAsync(result.Match(Results.Ok, ApiResults.Problem));
    }
}

