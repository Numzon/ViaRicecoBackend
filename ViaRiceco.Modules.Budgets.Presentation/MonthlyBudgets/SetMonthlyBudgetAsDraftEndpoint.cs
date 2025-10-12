using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.SetMonthlyBudgetAsDraft;
using ViaRiceco.Modules.Budgets.Presentation.Enumerations;

namespace ViaRiceco.Modules.Budgets.Presentation.MonthlyBudgets;

internal sealed class SetMonthlyBudgetAsDraftEndpoint(ISender sender)
    : Ep.Req<SetMonthlyBudgetAsDraftEndpoint.Request>.Res<Result>
{
    [UsedImplicitly]
    internal sealed record Request(string Id);

    public override void Configure()
    {
        Post("/budgets/monthly-budgets/{id}/set-as-draft");
        AllowAnonymous();
        Description(d =>
        {
            d.WithName(nameof(SetMonthlyBudgetAsDraftEndpoint));
            d.Accepts<Request>();
        });
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.MonthlyBudgets)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new SetMonthlyBudgetAsDraftCommand(req.Id);
        Result result = await sender.Send(command, ct);

        await Send.ResultAsync(result.Match(Results.NoContent, ApiResults.Problem));
    }
}
