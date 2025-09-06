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
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.GetSettlementPeriods;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models;
using ViaRiceco.Modules.Accounting.Presentation.Enumerations;
using ViaRiceco.Modules.Accounting.Presentation.SettlementPeriods.Hyperlinks;

namespace ViaRiceco.Modules.Accounting.Presentation.SettlementPeriods;

[UsedImplicitly]
public sealed class SettlementPeriodCollectionQueryParameters : CollectionQueryParameters
{
    public int? Month { get; init; }
    public int? Year { get; init; }
}

internal sealed class GetSettlementPeriodsEndpoint(
    ISender sender,
    IHyperlinkService hyperlinkService,
    IDataShapingService dataShapingService)
    : Ep.Req<SettlementPeriodCollectionQueryParameters>.Res<ViaRicecoCollectionResponse>
{
    
    public override void Configure()
    {
        Get("/accounting/settlement-periods");
        Tags(EndpointTags.SettlementPeriods);
        AllowAnonymous();
        Description(d => d.WithName(nameof(GetSettlementPeriodsEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.SettlementPeriods)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(SettlementPeriodCollectionQueryParameters collectionQuery, CancellationToken ct)
    {
        var queryCommand = new GetSettlementPeriodsQuery(collectionQuery.Search, collectionQuery.Sort, collectionQuery.Page,
            collectionQuery.PageSize, collectionQuery.Month, collectionQuery.Year);
        Result<GetSettlementPeriodsQueryResponse> result = await sender.Send(queryCommand, ct);

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
            Hyperlink[] links = SettlementPeriodsHyperlinks.CreateSettlementPeriodCollectionLinks(hyperlinkService, collectionQuery,
                collectionResponse.HasNextPage,
                collectionResponse.HasPreviousPage);

            collectionResponse.Links = links;    
        }
        
        await Send.ResultAsync(Results.Ok(collectionResponse));
    }

    private IReadOnlyCollection<ExpandoObject> ShapeCollectionData(IReadOnlyCollection<SettlementPeriodDto> items, string? fields, bool includeLinks)
    {
        if (includeLinks)
        {
            return dataShapingService.ShapeCollectionData(items, fields,
                    x => SettlementPeriodsHyperlinks.CreateSettlementPeriodItemLinks(hyperlinkService, x.Id));    
        }
        
        return dataShapingService.ShapeCollectionData(items, fields);
    }
}
