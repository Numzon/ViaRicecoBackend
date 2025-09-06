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
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.AddTax;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models;
using ViaRiceco.Modules.Accounting.Presentation.Enumerations;
using ViaRiceco.Modules.Accounting.Presentation.SettlementPeriods.Hyperlinks;

namespace ViaRiceco.Modules.Accounting.Presentation.SettlementPeriods;

internal sealed class AddTaxEndpoint(ISender sender, IHyperlinkService hyperlinkService, IDataShapingService dataShapingService)
    : Ep.Req<AddTaxEndpoint.Request>.Res<Result<TaxDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public string SettlementPeriodId { get; init; }
        public decimal Value { get; init; }
        public string TaxTypeId { get; init; }
    }

    public override void Configure()
    {
        Post("/accounting/settlement-periods/{settlementPeriodId}/taxes");
        Tags(EndpointTags.SettlementPeriods);
        AllowAnonymous();
        Description(d => d.WithName(nameof(AddTaxEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.SettlementPeriods)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new AddTaxCommand(req.SettlementPeriodId, req.Value, req.TaxTypeId);
        Result<TaxDto> result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        ExpandoObject shapedObject = ShapeDataWithConditionalLinks(result.Value, req.IncludeLinks, req.SettlementPeriodId, result.Value.Id);
        
        await Send.ResultAsync(Results.Ok(shapedObject));
    }

    private ExpandoObject ShapeDataWithConditionalLinks(TaxDto data, bool includeLinks, string settlementPeriodId, string taxId)
    {
        return includeLinks
            ? dataShapingService.ShapeData(data, null, SettlementPeriodsHyperlinks.CreateTaxItemLinks(hyperlinkService, settlementPeriodId, taxId))
            : dataShapingService.ShapeData(data, null);
    }
}
