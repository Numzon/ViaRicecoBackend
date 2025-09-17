using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Presentation.Abstractions.Collections;

namespace ViaRiceco.Modules.Portfolios.Presentation.FinancialGoals.Hyperlinks;

public static class FinancialGoalsHyperlinks
{
    public static Hyperlink[] CreateFinancialGoalItemLinks(IHyperlinkService hyperlinkService, string id)
    {
        return
        [
            hyperlinkService.Create(nameof(GetFinancialGoalEndpoint), RelationshipTypes.Self, HttpMethods.Get, new { id }),
            hyperlinkService.Create(nameof(UpdateFinancialGoalEndpoint), RelationshipTypes.Update, HttpMethods.Put, new { id }),
            hyperlinkService.Create(nameof(DeleteFinancialGoalEndpoint), RelationshipTypes.Delete, HttpMethods.Delete, new { id }),
            hyperlinkService.Create(nameof(GetFinancialGoalsEndpoint), RelationshipTypes.Collection, HttpMethods.Get)
        ];
    }
    
    public static Hyperlink[] CreateFinancialGoalCollectionLinks(IHyperlinkService hyperlinkService, ViaRicecoCollectionQueryParameters parameters, bool hasNextPage, bool hasPreviousPage)
    {
        List<Hyperlink> hyperlinks =
        [
            hyperlinkService.Create(nameof(GetFinancialGoalsEndpoint), RelationshipTypes.Self, HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, parameters.Page, parameters.PageSize
                }),
            hyperlinkService.Create(nameof(CreateFinancialGoalEndpoint), RelationshipTypes.Create, HttpMethods.Post)
        ];

        if (hasNextPage)
        {
            hyperlinks.Add(hyperlinkService.Create(nameof(GetFinancialGoalsEndpoint), RelationshipTypes.NextPage,
                HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, page = parameters.Page + 1,
                    parameters.PageSize
                }));
        }

        if (hasPreviousPage)
        {
            hyperlinks.Add(hyperlinkService.Create(nameof(GetFinancialGoalsEndpoint), RelationshipTypes.PreviousPage,
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
