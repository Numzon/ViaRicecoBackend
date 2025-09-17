using System.Dynamic;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Services.DataShapers;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Abstractions.Headers;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Accounting.Presentation.Enumerations;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.CreateSettlementPeriod;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models;
using ViaRiceco.Modules.Accounting.Presentation.SettlementPeriods.Hyperlinks;

namespace ViaRiceco.Modules.Accounting.Presentation.SettlementPeriods;

internal sealed class CreateSettlementPeriodEndpoint(ISender sender, IHyperlinkService hyperlinkService, IDataShapingService dataShapingService)
    : Ep.Req<CreateSettlementPeriodEndpoint.Request>.Res<Result<SettlementPeriodDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public int Month { get; init; }
        public int Year { get; init; }
    }

    public override void Configure()
    {
        Post("/accounting/settlement-periods");
        AllowAnonymous();
        Description(d => d.WithName(nameof(CreateSettlementPeriodEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.SettlementPeriods)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new CreateSettlementPeriodCommand(req.Month, req.Year);
        Result<SettlementPeriodDto> result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        ExpandoObject shapedObject = ShapeDataWithConditionalLinks(result.Value, req.IncludeLinks, result.Value.Id);
        
        await Send.ResultAsync(Results.CreatedAtRoute(
            nameof(GetSettlementPeriodEndpoint), 
            new { id = result.Value.Id },   
            shapedObject));
    }

    private ExpandoObject ShapeDataWithConditionalLinks(SettlementPeriodDto data, bool includeLinks, string settlementPeriodId)
    {
        return includeLinks
            ? dataShapingService.ShapeData(data, null, SettlementPeriodsHyperlinks.CreateSettlementPeriodItemLinks(hyperlinkService, settlementPeriodId))
            : dataShapingService.ShapeData(data, null);
    }
}
