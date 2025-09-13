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
using ViaRiceco.Common.Presentation.Enumerations;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.GetSettlementPeriod;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models;
using ViaRiceco.Modules.Accounting.Presentation.Enumerations;
using ViaRiceco.Modules.Accounting.Presentation.SettlementPeriods.Hyperlinks;

namespace ViaRiceco.Modules.Accounting.Presentation.SettlementPeriods;

internal sealed class GetSettlementPeriodEndpoint(
    ISender sender,
    IHyperlinkService hyperlinkService,
    IDataShapingService dataShapingService)
    : Ep.Req<GetSettlementPeriodEndpoint.Request>.Res<Result<SettlementPeriodDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public string Id { get; init; }
        public string? Fields { get; init; }
    };

    public override void Configure()
    {
        Get("/accounting/settlement-periods/{id}");
        AllowAnonymous();
        Description(d => d.WithName(nameof(GetSettlementPeriodEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.SettlementPeriods)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var query = new GetSettlementPeriodQuery(req.Id);
        Result<SettlementPeriodDto> result = await sender.Send(query, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }

        ExpandoObject shapedObject = ShapeDataWithConditionalLinks(result.Value, req.IncludeLinks, result.Value.Id, req.Fields);

        await Send.ResultAsync(Results.Ok(shapedObject));
    }

    private ExpandoObject ShapeDataWithConditionalLinks(SettlementPeriodDto data, bool includeLinks, string settlementPeriodId, string? fields = null)
    {
        return includeLinks
            ? dataShapingService.ShapeData(data, fields, SettlementPeriodsHyperlinks.CreateSettlementPeriodItemLinks(hyperlinkService, settlementPeriodId))
            : dataShapingService.ShapeData(data, fields);
    }
}
