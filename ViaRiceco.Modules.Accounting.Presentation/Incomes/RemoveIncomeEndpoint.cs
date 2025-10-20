using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Accounting.Application.Incomes.RemoveIncome;
using ViaRiceco.Modules.Accounting.Presentation.Enumerations;

namespace ViaRiceco.Modules.Accounting.Presentation.Incomes;

internal sealed class RemoveIncomeEndpoint(ISender sender)
    : Ep.Req<RemoveIncomeEndpoint.Request>.Res<Result>
{
    [UsedImplicitly]
    internal sealed record Request(string SettlementPeriodId, string IncomeId);

    public override void Configure()
    {
        Delete("/accounting/settlement-periods/{settlementPeriodId}/incomes/{incomeId}");
        AllowAnonymous();
        Description(d => d.WithName(nameof(RemoveIncomeEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.SettlementPeriods)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new RemoveIncomeCommand(req.SettlementPeriodId, req.IncomeId);
        Result result = await sender.Send(command, ct);

        await Send.ResultAsync(result.Match(Results.NoContent, ApiResults.Problem));
    }
}

