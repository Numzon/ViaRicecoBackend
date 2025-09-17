using System.Dynamic;
using FastEndpoints;
using FastEndpoints.AspVersioning;
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
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategyTypes.GetInvestmentStrategyTypes;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategyTypes.Models;
using ViaRiceco.Modules.Portfolios.Presentation.Enumerations;
using ViaRiceco.Modules.Portfolios.Presentation.InvestmentStrategyTypes.Hyperlinks;

namespace ViaRiceco.Modules.Portfolios.Presentation.InvestmentStrategyTypes;

internal sealed class GetInvestmentStrategyTypesEndpoint(
    ISender sender,
    IHyperlinkService hyperlinkService,
    IDataShapingService dataShapingService)
    : Ep.Req<ViaRicecoCollectionQueryParameters>.Res<ViaRicecoCollectionResponse>
{
    public override void Configure()
    {
        Get("/portfolios/investment-strategy-types");
        AllowAnonymous();
        Description(d => d.WithName(nameof(GetInvestmentStrategyTypesEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.InvestmentStrategyTypes)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(ViaRicecoCollectionQueryParameters collectionQuery, CancellationToken ct)
    {
        var queryCommand = new GetInvestmentStrategyTypesQuery(collectionQuery.Search, collectionQuery.Sort, collectionQuery.Page,
            collectionQuery.PageSize);
        Result<GetInvestmentStrategyTypesQueryResponse> result = await sender.Send(queryCommand, ct);

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
            Hyperlink[] links = InvestmentStrategyTypesHyperlinks.CreateInvestmentStrategyTypeCollectionLinks(hyperlinkService, collectionQuery,
                collectionResponse.HasNextPage,
                collectionResponse.HasPreviousPage);

            collectionResponse.Links = links;    
        }
        
        await Send.ResultAsync(Results.Ok(collectionResponse));
    }

    private IReadOnlyCollection<ExpandoObject> ShapeCollectionData(IReadOnlyCollection<InvestmentStrategyTypeDto> items, string? fields, bool includeLinks)
    {
        if (includeLinks)
        {
            return dataShapingService.ShapeCollectionData(items, fields,
                    x => InvestmentStrategyTypesHyperlinks.CreateInvestmentStrategyTypeItemLinks(hyperlinkService, x.Id));    
        }
        
        return dataShapingService.ShapeCollectionData(items, fields);
    }
}
