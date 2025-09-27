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
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.GetExpenseTypes;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Budgets.Presentation.Enumerations;
using ViaRiceco.Modules.Budgets.Presentation.ExpenseTypes.Hyperlinks;

namespace ViaRiceco.Modules.Budgets.Presentation.ExpenseTypes;

internal sealed class GetExpenseTypesEndpoint(
    ISender sender,
    IHyperlinkService hyperlinkService,
    IDataShapingService dataShapingService)
    : Ep.Req<ViaRicecoCollectionQueryParameters>.Res<ViaRicecoCollectionResponse>
{
    public override void Configure()
    {
        Get("/budgets/expense-types");
        AllowAnonymous();
        Description(d => d.WithName(nameof(GetExpenseTypesEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.ExpenseTypes)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(ViaRicecoCollectionQueryParameters collectionQuery, CancellationToken ct)
    {
        var queryCommand = new GetExpenseTypesQuery(collectionQuery.Search, collectionQuery.Sort, collectionQuery.Page,
            collectionQuery.PageSize);
        Result<GetExpenseTypesQueryResponse> result = await sender.Send(queryCommand, ct);

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
            Hyperlink[] links = ExpenseTypesHyperlinks.CreateExpenseTypeCollectionLinks(hyperlinkService, collectionQuery,
                collectionResponse.HasNextPage,
                collectionResponse.HasPreviousPage);

            collectionResponse.Links = links;    
        }
        
        await Send.ResultAsync(Results.Ok(collectionResponse));
    }

    private IReadOnlyCollection<ExpandoObject> ShapeCollectionData(IReadOnlyCollection<ExpenseTypeDto> items, string? fields, bool includeLinks)
    {
        if (includeLinks)
        {
            return dataShapingService.ShapeCollectionData(items, fields,
                    x => ExpenseTypesHyperlinks.CreateExpenseTypeItemLinks(hyperlinkService, x.Id));    
        }
        
        return dataShapingService.ShapeCollectionData(items, fields);
    }
}
