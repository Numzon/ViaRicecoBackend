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
using ViaRiceco.Common.Presentation.Abstractions.Headers;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Budgets.Application.Expenses.GetExpenses;
using ViaRiceco.Modules.Budgets.Application.Expenses.Models;
using ViaRiceco.Modules.Budgets.Presentation.Enumerations;
using ViaRiceco.Modules.Budgets.Presentation.Expenses.Hyperlinks;

namespace ViaRiceco.Modules.Budgets.Presentation.Expenses;

internal sealed class GetExpensesEndpoint(
    ISender sender,
    IHyperlinkService hyperlinkService,
    IDataShapingService dataShapingService)
    : Ep.Req<GetExpensesEndpoint.ExpenseCollectionQueryParameters>.Res<ViaRicecoCollectionResponse>
{
    [UsedImplicitly]
    internal sealed class ExpenseCollectionQueryParameters : CollectionQueryParameters
    {
        public string? ExpenseTypeId { get; init; }
    }

    public override void Configure()
    {
        Get("/budgets/expenses");
        AllowAnonymous();
        Description(d => d.WithName(nameof(GetExpensesEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.Expenses)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(ExpenseCollectionQueryParameters req, CancellationToken ct)
    {
        var queryCommand = new GetExpensesQuery(req.Search, req.Sort, req.Page, req.PageSize, req.ExpenseTypeId);
        Result<GetExpensesQueryResponse> result = await sender.Send(queryCommand, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        IReadOnlyCollection<ExpandoObject> shapedCollection = ShapeCollectionData(result.Value.Items, req.Fields, req.IncludeLinks);

        var collectionResponse = new ViaRicecoCollectionResponse
        {
            Items = shapedCollection,
            Page = req.Page,
            TotalCount = result.Value.TotalCount,
            PageSize = req.PageSize,
        };

        if (req.IncludeLinks)
        {
            var parameters = new ViaRicecoCollectionQueryParameters
            {
                Search = req.Search,
                Sort = req.Sort,
                Page = req.Page,
                PageSize = req.PageSize,
                Fields = req.Fields
            };

            Hyperlink[] links = ExpensesHyperlinks.CreateExpenseCollectionLinks(hyperlinkService, parameters, req.ExpenseTypeId,
                collectionResponse.HasNextPage,
                collectionResponse.HasPreviousPage);

            collectionResponse.Links = links;    
        }
        
        await Send.ResultAsync(Results.Ok(collectionResponse));
    }

    private IReadOnlyCollection<ExpandoObject> ShapeCollectionData(IReadOnlyCollection<ExpenseDto> items, string? fields, bool includeLinks)
    {
        if (includeLinks)
        {
            return dataShapingService.ShapeCollectionData(items, fields,
                    x => ExpensesHyperlinks.CreateExpenseItemLinks(hyperlinkService, x.Id));    
        }
        
        return dataShapingService.ShapeCollectionData(items, fields);
    }
}
