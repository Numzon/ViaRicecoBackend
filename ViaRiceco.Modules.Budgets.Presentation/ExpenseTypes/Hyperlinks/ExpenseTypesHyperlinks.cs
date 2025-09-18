using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Presentation.Abstractions.Collections;

namespace ViaRiceco.Modules.Budgets.Presentation.ExpenseTypes.Hyperlinks;

public static class ExpenseTypesHyperlinks
{
    public static Hyperlink[] CreateExpenseTypeItemLinks(IHyperlinkService hyperlinkService, string id)
    {
        return
        [
            hyperlinkService.Create(nameof(GetExpenseTypeEndpoint), RelationshipTypes.Self, HttpMethods.Get, new { id }),
            hyperlinkService.Create(nameof(UpdateExpenseTypeEndpoint), RelationshipTypes.Update, HttpMethods.Put, new { id }),
            hyperlinkService.Create(nameof(DeleteExpenseTypeEndpoint), RelationshipTypes.Delete, HttpMethods.Delete, new { id }),
            hyperlinkService.Create(nameof(GetExpenseTypesEndpoint), RelationshipTypes.Collection, HttpMethods.Get)
        ];
    }
    
    public static Hyperlink[] CreateExpenseTypeCollectionLinks(IHyperlinkService hyperlinkService, ViaRicecoCollectionQueryParameters parameters, bool hasNextPage, bool hasPreviousPage)
    {
        List<Hyperlink> hyperlinks =
        [
            hyperlinkService.Create(nameof(GetExpenseTypesEndpoint), RelationshipTypes.Self, HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, parameters.Page, parameters.PageSize
                }),
            hyperlinkService.Create(nameof(CreateExpenseTypeEndpoint), RelationshipTypes.Create, HttpMethods.Post)
        ];

        if (hasNextPage)
        {
            hyperlinks.Add(hyperlinkService.Create(nameof(GetExpenseTypesEndpoint), RelationshipTypes.NextPage,
                HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, page = parameters.Page + 1,
                    parameters.PageSize
                }));
        }

        if (hasPreviousPage)
        {
            hyperlinks.Add(hyperlinkService.Create(nameof(GetExpenseTypesEndpoint), RelationshipTypes.PreviousPage,
                HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, page = parameters.Page - 1,
                    parameters.PageSize
                }));
        }

        return [.. hyperlinks];
    }
}
