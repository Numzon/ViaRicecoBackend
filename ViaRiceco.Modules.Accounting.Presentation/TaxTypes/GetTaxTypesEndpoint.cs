using System.Dynamic;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Abstractions.Collections;
using ViaRiceco.Common.Application.Services.DataShapers;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.GetTaxTypes;

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
        Tags("TaxTypes");
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
            dataShapingService.ShapeCollectionData(result.Value.Items, collectionQuery.Fields, x => GetLinks(x.Id));

        var collectionResponse = new ViaRicecoCollectionResponse
        {
            Items = shapedCollection,
            Page = collectionQuery.Page,
            TotalCount = result.Value.TotalCount,
            PageSize = collectionQuery.PageSize,
        };

        Hyperlink[] links = GetLinks(collectionQuery, collectionResponse.HasNextPage,
            collectionResponse.HasPreviousPage);

        collectionResponse.Links = links;

        await Send.ResultAsync(Results.Ok(collectionResponse));
    }

    private Hyperlink[] GetLinks(ViaRicecoCollectionQueryParameters parameters, bool hasNextPage, bool hasPreviousPage)
    {
        List<Hyperlink> hyperlinks =
        [
            hyperlinkService.Create(nameof(GetTaxTypesEndpoint), "self", HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, parameters.Page, parameters.PageSize
                }),
            hyperlinkService.Create(nameof(CreateTaxTypeEndpoint), "create", HttpMethods.Post)
        ];

        if (hasNextPage)
        {
            hyperlinks.Add(hyperlinkService.Create(nameof(GetTaxTypesEndpoint), "next-page", HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, page = parameters.Page + 1,
                    parameters.PageSize
                }));
        }

        if (hasPreviousPage)
        {
            hyperlinks.Add(hyperlinkService.Create(nameof(GetTaxTypesEndpoint), "prev-page", HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, page = parameters.Page - 1,
                    parameters.PageSize
                }));
        }

        return [.. hyperlinks];
    }

    private Hyperlink[] GetLinks(string id)
    {
        return
        [
            hyperlinkService.Create(nameof(GetTaxTypeEndpoint), "self", HttpMethods.Get, new { id }),
            hyperlinkService.Create(nameof(UpdateTaxTypeEndpoint), "update", HttpMethods.Put, new { id }),
            hyperlinkService.Create(nameof(DeleteTaxTypeEndpoint), "delete", HttpMethods.Delete, new { id }),
            hyperlinkService.Create(nameof(GetTaxTypesEndpoint), "collection", HttpMethods.Get)
        ];
    }
}
