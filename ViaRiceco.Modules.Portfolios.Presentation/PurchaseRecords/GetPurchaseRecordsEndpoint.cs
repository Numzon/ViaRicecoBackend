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
using ViaRiceco.Common.Presentation.Abstractions.Collections;
using ViaRiceco.Common.Presentation.Enumerations;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.GetPurchaseRecords;
using ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.Models;
using ViaRiceco.Modules.Portfolios.Presentation.Enumerations;
using ViaRiceco.Modules.Portfolios.Presentation.PurchaseRecords.Hyperlinks;

namespace ViaRiceco.Modules.Portfolios.Presentation.PurchaseRecords;

internal sealed class GetPurchaseRecordsEndpoint(
    ISender sender,
    IHyperlinkService hyperlinkService,
    IDataShapingService dataShapingService)
    : Ep.Req<GetPurchaseRecordsEndpoint.Request>.Res<ViaRicecoCollectionResponse>
{
    public override void Configure()
    {
        Get("/portfolios/purchase-records");
        Tags(EndpointTags.PurchaseRecords);
        AllowAnonymous();
        Description(d => d.WithName(nameof(GetPurchaseRecordsEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.PurchaseRecords)
            .MapToApiVersion(1.0));
    }

    [UsedImplicitly]
    internal sealed class Request : CollectionQueryParameters
    {
        public string? InvestmentId { get; init; }
        public string? CurrencyId { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
    }

    public override async Task HandleAsync(Request collectionQuery, CancellationToken ct)
    {
        var queryCommand = new GetPurchaseRecordsQuery(
            collectionQuery.Search, 
            collectionQuery.Sort, 
            collectionQuery.Page,
            collectionQuery.PageSize, 
            collectionQuery.InvestmentId, 
            collectionQuery.CurrencyId, 
            collectionQuery.FromDate, 
            collectionQuery.ToDate);
        Result<GetPurchaseRecordsQueryResponse> result = await sender.Send(queryCommand, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        IReadOnlyCollection<ExpandoObject> shapedCollection = ShapeCollectionData(result.Value.Items, collectionQuery.Fields, collectionQuery.IncludeLinks);

        var collectionResponse = new ViaRicecoCollectionResponse
        {
            Items = shapedCollection,
            Page = collectionQuery.Page,
            TotalCount = result.Value.TotalCount,
            PageSize = collectionQuery.PageSize,
        };

        if (collectionQuery.IncludeLinks)
        {
            var queryParameters = new ViaRicecoCollectionQueryParameters
            {
                Search = collectionQuery.Search,
                Sort = collectionQuery.Sort,
                Fields = collectionQuery.Fields,
                Page = collectionQuery.Page,
                PageSize = collectionQuery.PageSize
            };
            
            Hyperlink[] links = PurchaseRecordsHyperlinks.CreatePurchaseRecordCollectionLinks(hyperlinkService, queryParameters,
                collectionResponse.HasNextPage,
                collectionResponse.HasPreviousPage);

            collectionResponse.Links = links;    
        }
        
        await Send.ResultAsync(Results.Ok(collectionResponse));
    }

    private IReadOnlyCollection<ExpandoObject> ShapeCollectionData(IReadOnlyCollection<PurchaseRecordDto> items, string? fields, bool includeLinks)
    {
        if (includeLinks)
        {
            return dataShapingService.ShapeCollectionData(items, fields,
                    x => PurchaseRecordsHyperlinks.CreatePurchaseRecordItemLinks(hyperlinkService, x.Id));    
        }
        
        return dataShapingService.ShapeCollectionData(items, fields);
    }
}
