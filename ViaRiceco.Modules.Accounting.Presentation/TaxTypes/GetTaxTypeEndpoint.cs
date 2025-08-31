using System.Dynamic;
using FastEndpoints;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Services.DataShapers;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.GetTaxType;
using ViaRiceco.Modules.Accounting.Presentation.Enumerations;

namespace ViaRiceco.Modules.Accounting.Presentation.TaxTypes;

internal sealed class GetTaxTypeEndpoint(
    ISender sender,
    IHyperlinkService hyperlinkService,
    IDataShapingService dataShapingService)
    : Ep.Req<GetTaxTypeEndpoint.Request>.Res<Result<TaxTypeDto>>
{
    [UsedImplicitly]
    internal sealed record Request(string Id, string? Fields);

    public override void Configure()
    {
        Get("/accounting/tax-types/{id}");
        Tags(EndpointTags.TaxTypes);
        AllowAnonymous();
        Description(d => d.WithName(nameof(GetTaxTypeEndpoint)));
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

        Hyperlink[] links = TaxTypesHyperlinks.CreateTaxTypeItemLinks(hyperlinkService, result.Value.Id);
        ExpandoObject shapedObject = dataShapingService.ShapeData(result.Value, req.Fields, links);

        await Send.ResultAsync(Results.Ok(shapedObject));
    }
}
