using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Presentation.Abstractions.Collections;

namespace ViaRiceco.Modules.Portfolios.Presentation.InvestmentStrategyTypes.Hyperlinks;

public static class InvestmentStrategyTypesHyperlinks
{
    public static Hyperlink[] CreateInvestmentStrategyTypeItemLinks(IHyperlinkService hyperlinkService, string id)
    {
        return
        [
            hyperlinkService.Create(nameof(GetInvestmentStrategyTypeEndpoint), RelationshipTypes.Self, HttpMethods.Get, new { id }),
            hyperlinkService.Create(nameof(UpdateInvestmentStrategyTypeEndpoint), RelationshipTypes.Update, HttpMethods.Put, new { id }),
            hyperlinkService.Create(nameof(DeleteInvestmentStrategyTypeEndpoint), RelationshipTypes.Delete, HttpMethods.Delete, new { id }),
            hyperlinkService.Create(nameof(GetInvestmentStrategyTypesEndpoint), RelationshipTypes.Collection, HttpMethods.Get)
        ];
    }
    
    public static Hyperlink[] CreateInvestmentStrategyTypeCollectionLinks(IHyperlinkService hyperlinkService, ViaRicecoCollectionQueryParameters parameters, bool hasNextPage, bool hasPreviousPage)
    {
        List<Hyperlink> hyperlinks =
        [
            hyperlinkService.Create(nameof(GetInvestmentStrategyTypesEndpoint), RelationshipTypes.Self, HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, parameters.Page, parameters.PageSize
                }),
            hyperlinkService.Create(nameof(CreateInvestmentStrategyTypeEndpoint), RelationshipTypes.Create, HttpMethods.Post)
        ];

        if (hasNextPage)
        {
            hyperlinks.Add(hyperlinkService.Create(nameof(GetInvestmentStrategyTypesEndpoint), RelationshipTypes.NextPage,
                HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, page = parameters.Page + 1,
                    parameters.PageSize
                }));
        }

        if (hasPreviousPage)
        {
            hyperlinks.Add(hyperlinkService.Create(nameof(GetInvestmentStrategyTypesEndpoint), RelationshipTypes.PreviousPage,
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
