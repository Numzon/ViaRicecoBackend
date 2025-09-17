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
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.GetInvestmentStrategies;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.Models;
using ViaRiceco.Modules.Portfolios.Presentation.Enumerations;
using ViaRiceco.Modules.Portfolios.Presentation.InvestmentStrategies.Hyperlinks;

namespace ViaRiceco.Modules.Portfolios.Presentation.InvestmentStrategies;

internal sealed class GetInvestmentStrategiesEndpoint(
    ISender sender,
    IHyperlinkService hyperlinkService,
    IDataShapingService dataShapingService)
    : Ep.Req<GetInvestmentStrategiesEndpoint.Request>.Res<ViaRicecoCollectionResponse>
{
    public override void Configure()
    {
        Get("/portfolios/investment-strategies");
        AllowAnonymous();
        Description(d => d.WithName(nameof(GetInvestmentStrategiesEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.InvestmentStrategies)
            .MapToApiVersion(1.0));
    }

    [UsedImplicitly]
    internal sealed class Request : CollectionQueryParameters
    {
        public string? FinancialGoalId { get; init; }
        public string? InvestmentStrategyTypeId { get; init; }
    }

    public override async Task HandleAsync(Request collectionQuery, CancellationToken ct)
    {
        var queryCommand = new GetInvestmentStrategiesQuery(
            collectionQuery.Search, 
            collectionQuery.Sort, 
            collectionQuery.Page,
            collectionQuery.PageSize, 
            collectionQuery.FinancialGoalId, 
            collectionQuery.InvestmentStrategyTypeId);
        Result<GetInvestmentStrategiesQueryResponse> result = await sender.Send(queryCommand, ct);

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
            
            Hyperlink[] links = InvestmentStrategiesHyperlinks.CreateInvestmentStrategyCollectionLinks(hyperlinkService, queryParameters,
                collectionResponse.HasNextPage,
                collectionResponse.HasPreviousPage);

            collectionResponse.Links = links;    
        }
        
        await Send.ResultAsync(Results.Ok(collectionResponse));
    }

    private IReadOnlyCollection<ExpandoObject> ShapeCollectionData(IReadOnlyCollection<InvestmentStrategyDto> items, string? fields, bool includeLinks)
    {
        if (includeLinks)
        {
            return dataShapingService.ShapeCollectionData(items, fields,
                    x => InvestmentStrategiesHyperlinks.CreateInvestmentStrategyItemLinks(hyperlinkService, x.Id));    
        }
        
        return dataShapingService.ShapeCollectionData(items, fields);
    }
}
