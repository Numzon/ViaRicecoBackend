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
    : Ep.NoReq.Res<Result>
{

    public override void Configure()
    {
        Post("/budgets/monthly-budgets/{id}/finalize");
        AllowAnonymous();
        DontAutoTag(); // No request body expected
        Description(d =>
        {
            d.WithName(nameof(FinalizeMonthlyBudgetEndpoint));
        });

        Options(x => x
            .WithVersionSet(CustomVersionSets.MonthlyBudgets)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        string id = Route<string>("id")!;
        var command = new FinalizeMonthlyBudgetCommand(id);
        Result result = await sender.Send(command, ct);

        await Send.ResultAsync(result.Match(Results.NoContent, ApiResults.Problem));
    }
}
