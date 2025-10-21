using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.FinalizeSettlementPeriod;
using ViaRiceco.Modules.Accounting.Presentation.Enumerations;

namespace ViaRiceco.Modules.Accounting.Presentation.SettlementPeriods;

internal sealed class FinalizeSettlementPeriodEndpoint(ISender sender)
    : Ep.Req<FinalizeSettlementPeriodEndpoint.Request>.Res<Result>
{
    [UsedImplicitly]
    internal sealed record Request(string Id);

    public override void Configure()
    {
        Post("/accounting/settlement-periods/{id}/finalize");
        AllowAnonymous();
        Description(d =>
        {
            d.WithName(nameof(FinalizeSettlementPeriodEndpoint));
            d.Accepts<Request>();
        });
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.SettlementPeriods)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new FinalizeSettlementPeriodCommand(req.Id);
        Result result = await sender.Send(command, ct);

        await Send.ResultAsync(result.Match(Results.NoContent, ApiResults.Problem));
    }
}
