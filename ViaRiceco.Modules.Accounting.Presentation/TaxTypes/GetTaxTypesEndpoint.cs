using System.Dynamic;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Abstractions.Collections;
using ViaRiceco.Common.Application.Services.DataShapers;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.GetTaxTypes;
using ViaRiceco.Modules.Accounting.Presentation.Enumerations;
using ViaRiceco.Modules.Accounting.Presentation.TaxTypes.Hyperlinks;

namespace ViaRiceco.Modules.Accounting.Presentation.TaxTypes;

internal sealed class GetTaxTypesEndpoint(
    ISender sender,
    IHyperlinkService hyperlinkService,
    IDataShapingService dataShapingService)
    : Ep.Req<ViaRicecoCollectionQueryParameters>.Res<ViaRicecoCollectionResponse>
{
    public override void Configure()
    {
        Get("/accounting/tax-types");
        Tags(EndpointTags.TaxTypes);
        AllowAnonymous();
        Description(d => d.WithName(nameof(GetTaxTypesEndpoint)));
    }

    public override async Task HandleAsync(ViaRicecoCollectionQueryParameters collectionQuery, CancellationToken ct)
    {
        var queryCommand = new GetTaxTypesQuery(collectionQuery.Search, collectionQuery.Sort, collectionQuery.Page,
            collectionQuery.PageSize);
        Result<GetTaxTypesQueryResponse> result = await sender.Send(queryCommand, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }

        IReadOnlyCollection<ExpandoObject> shapedCollection =
            dataShapingService.ShapeCollectionData(result.Value.Items, collectionQuery.Fields,
                x => TaxTypesHyperlinks.CreateTaxTypeItemLinks(hyperlinkService, x.Id));

        var collectionResponse = new ViaRicecoCollectionResponse
        {
            Items = shapedCollection,
            Page = collectionQuery.Page,
            TotalCount = result.Value.TotalCount,
            PageSize = collectionQuery.PageSize,
        };

        Hyperlink[] links = TaxTypesHyperlinks.CreateTaxTypeCollectionLinks(hyperlinkService, collectionQuery,
            collectionResponse.HasNextPage,
            collectionResponse.HasPreviousPage);

        collectionResponse.Links = links;

        await Send.ResultAsync(Results.Ok(collectionResponse));
    }
}
