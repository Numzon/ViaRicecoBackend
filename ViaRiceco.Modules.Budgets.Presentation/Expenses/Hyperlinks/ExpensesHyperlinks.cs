using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Presentation.Abstractions.Collections;

namespace ViaRiceco.Modules.Budgets.Presentation.Expenses.Hyperlinks;

public static class ExpensesHyperlinks
{
    public static Hyperlink[] CreateExpenseItemLinks(IHyperlinkService hyperlinkService, string id)
    {
        return
        [
            hyperlinkService.Create(nameof(GetExpenseEndpoint), RelationshipTypes.Self, HttpMethods.Get, new { id }),
            hyperlinkService.Create(nameof(UpdateExpenseEndpoint), RelationshipTypes.Update, HttpMethods.Put, new { id }),
            hyperlinkService.Create(nameof(DeleteExpenseEndpoint), RelationshipTypes.Delete, HttpMethods.Delete, new { id }),
            hyperlinkService.Create(nameof(GetExpensesEndpoint), RelationshipTypes.Collection, HttpMethods.Get)
        ];
    }
    
    public static Hyperlink[] CreateExpenseCollectionLinks(IHyperlinkService hyperlinkService, ViaRicecoCollectionQueryParameters parameters, string? expenseTypeId, bool hasNextPage, bool hasPreviousPage)
    {
        List<Hyperlink> hyperlinks =
        [
            hyperlinkService.Create(nameof(GetExpensesEndpoint), RelationshipTypes.Self, HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, parameters.Page, parameters.PageSize,
                    expenseTypeId
                }),
            hyperlinkService.Create(nameof(CreateExpenseEndpoint), RelationshipTypes.Create, HttpMethods.Post)
        ];

        if (hasNextPage)
        {
            hyperlinks.Add(hyperlinkService.Create(nameof(GetExpensesEndpoint), RelationshipTypes.NextPage,
                HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, page = parameters.Page + 1,
                    parameters.PageSize,
                    expenseTypeId
                }));
        }

        if (hasPreviousPage)
        {
            hyperlinks.Add(hyperlinkService.Create(nameof(GetExpensesEndpoint), RelationshipTypes.PreviousPage,
                HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, page = parameters.Page - 1,
                    parameters.PageSize,
                    expenseTypeId
                }));
        }

        return [.. hyperlinks];
    }
}
