using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Enumerations;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.RemoveTax;
using ViaRiceco.Modules.Accounting.Presentation.Enumerations;

namespace ViaRiceco.Modules.Accounting.Presentation.SettlementPeriods;

internal sealed class RemoveTaxEndpoint(ISender sender)
    : Ep.Req<RemoveTaxEndpoint.Request>.Res<Result>
{
    [UsedImplicitly]
    internal sealed record Request(string SettlementPeriodId, string TaxId);

    public override void Configure()
    {
        Delete("/accounting/settlement-periods/{settlementPeriodId}/taxes/{taxId}");
        Tags(EndpointTags.SettlementPeriods);
        AllowAnonymous();
        Description(d => d.WithName(nameof(RemoveTaxEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.SettlementPeriods)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new RemoveTaxCommand(req.SettlementPeriodId, req.TaxId);
        Result result = await sender.Send(command, ct);

        await Send.ResultAsync(result.Match(Results.NoContent, ApiResults.Problem));
    }
}
