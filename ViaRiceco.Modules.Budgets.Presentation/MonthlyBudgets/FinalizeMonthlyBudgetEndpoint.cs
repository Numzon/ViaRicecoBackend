using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.FinalizeMonthlyBudget;
using ViaRiceco.Modules.Budgets.Presentation.Enumerations;

namespace ViaRiceco.Modules.Budgets.Presentation.MonthlyBudgets;

internal sealed class FinalizeMonthlyBudgetEndpoint(ISender sender)
    : Ep.Req<FinalizeMonthlyBudgetEndpoint.Request>.Res<Result>
{
    [UsedImplicitly]
    internal sealed record Request(string Id);

    public override void Configure()
    {
        Post("/budgets/monthly-budgets/{id}/finalize");
        AllowAnonymous();
        Description(d =>
        {
            d.WithName(nameof(FinalizeMonthlyBudgetEndpoint));
            d.Accepts<Request>();
        });

        Options(x => x
            .WithVersionSet(CustomVersionSets.MonthlyBudgets)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new FinalizeMonthlyBudgetCommand(req.Id);
        Result result = await sender.Send(command, ct);

        await Send.ResultAsync(result.Match(Results.NoContent, ApiResults.Problem));
    }
}
