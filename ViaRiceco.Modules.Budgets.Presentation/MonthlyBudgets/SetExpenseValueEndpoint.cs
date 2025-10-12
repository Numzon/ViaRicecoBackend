using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.SetExpenseValue;
using ViaRiceco.Modules.Budgets.Presentation.Enumerations;

namespace ViaRiceco.Modules.Budgets.Presentation.MonthlyBudgets;

internal sealed class SetExpenseValueEndpoint(ISender sender)
    : Ep.Req<SetExpenseValueEndpoint.Request>.Res<Result>
{
    [UsedImplicitly]
    internal sealed class Request
    {
        public string MonthlyBudgetId { get; init; } = string.Empty;
        public string MonthlyBudgetExpenseId { get; init; } = string.Empty;
        public decimal? Value { get; init; }
    }

    public override void Configure()
    {
        Put("/budgets/monthly-budgets/{monthlyBudgetId}/expenses/{monthlyBudgetExpenseId}/value");
        AllowAnonymous();
        Description(d =>
        {
            d.WithName(nameof(SetExpenseValueEndpoint));
        });
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.MonthlyBudgets)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new SetExpenseValueCommand(req.MonthlyBudgetId, req.MonthlyBudgetExpenseId, req.Value);
        Result result = await sender.Send(command, ct);

        await Send.ResultAsync(result.Match(Results.NoContent, ApiResults.Problem));
    }
}
