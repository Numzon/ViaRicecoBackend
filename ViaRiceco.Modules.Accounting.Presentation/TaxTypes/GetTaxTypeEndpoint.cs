using System.Dynamic;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Services.DataShapers;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Abstractions.Headers;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.GetTaxType;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Accounting.Presentation.Enumerations;
using ViaRiceco.Modules.Accounting.Presentation.TaxTypes.Hyperlinks;

namespace ViaRiceco.Modules.Accounting.Presentation.TaxTypes;

internal sealed class GetTaxTypeEndpoint(
    ISender sender,
    IHyperlinkService hyperlinkService,
    IDataShapingService dataShapingService)
    : Ep.Req<GetTaxTypeEndpoint.Request>.Res<Result<TaxTypeDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public string Id { get; init; }
        public string? Fields { get; init; }
    };

    public override void Configure()
    {
        Get("/accounting/tax-types/{id}");
        AllowAnonymous();
        Description(d => d.WithName(nameof(GetTaxTypeEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.TaxTypes)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var query = new GetTaxTypeQuery(req.Id);
        Result<TaxTypeDto> result = await sender.Send(query, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }

        ExpandoObject shapedObject = ShapeDataWithConditionalLinks(result.Value, req.IncludeLinks, result.Value.Id, req.Fields);

        await Send.ResultAsync(Results.Ok(shapedObject));
    }

    private ExpandoObject ShapeDataWithConditionalLinks(TaxTypeDto data, bool includeLinks, string taxTypeId, string? fields = null)
    {
        return includeLinks
            ? dataShapingService.ShapeData(data, fields, TaxTypesHyperlinks.CreateTaxTypeItemLinks(hyperlinkService, taxTypeId))
            : dataShapingService.ShapeData(data, fields);
    }
}
