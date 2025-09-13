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
using ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.GetPurchaseRecord;
using ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.Models;
using ViaRiceco.Modules.Portfolios.Presentation.Enumerations;
using ViaRiceco.Modules.Portfolios.Presentation.PurchaseRecords.Hyperlinks;

namespace ViaRiceco.Modules.Portfolios.Presentation.PurchaseRecords;

internal sealed class GetPurchaseRecordEndpoint(ISender sender, IHyperlinkService hyperlinkService, IDataShapingService dataShapingService)
    : Ep.Req<GetPurchaseRecordEndpoint.Request>.Res<Result<PurchaseRecordDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public string InvestmentStrategyId { get; init; }
        public string InvestmentId { get; init; }
        public string Id { get; init; }
    }

    public override void Configure()
    {
        Get("/portfolios/investment-strategies/{investmentStrategyId}/investments/{investmentId}/purchase-records/{id}");
        AllowAnonymous();
        Description(d => d.WithName(nameof(GetPurchaseRecordEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.PurchaseRecords)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var query = new GetPurchaseRecordQuery(req.InvestmentStrategyId, req.InvestmentId, req.Id);
        Result<PurchaseRecordDto> result = await sender.Send(query, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        ExpandoObject shapedObject = ShapeDataWithConditionalLinks(result.Value, req.IncludeLinks, result.Value.Id, req.InvestmentId, req.InvestmentStrategyId);
        
        await Send.ResultAsync(Results.Ok(shapedObject));
    }

    private ExpandoObject ShapeDataWithConditionalLinks(PurchaseRecordDto data, bool includeLinks, string purchaseRecordId, string investmentId, string investmentStrategyId, string? fields = null)
    {
        return includeLinks
            ? dataShapingService.ShapeData(data, fields, PurchaseRecordsHyperlinks.CreatePurchaseRecordItemLinks(hyperlinkService, purchaseRecordId, investmentId, investmentStrategyId))
            : dataShapingService.ShapeData(data, fields);
    }
}
