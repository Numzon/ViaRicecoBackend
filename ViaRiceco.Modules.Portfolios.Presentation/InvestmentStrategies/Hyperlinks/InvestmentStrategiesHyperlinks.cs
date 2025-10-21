using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Presentation.Abstractions.Collections;
using ViaRiceco.Modules.Portfolios.Presentation.Investments;

namespace ViaRiceco.Modules.Portfolios.Presentation.InvestmentStrategies.Hyperlinks;

public static class InvestmentStrategiesHyperlinks
{
    public static Hyperlink[] CreateInvestmentStrategyItemLinks(IHyperlinkService hyperlinkService, string id)
    {
        return
        [
            hyperlinkService.Create(nameof(GetInvestmentStrategyEndpoint), RelationshipTypes.Self, HttpMethods.Get, new { id }),
            hyperlinkService.Create(nameof(DeleteInvestmentStrategyEndpoint), RelationshipTypes.Delete, HttpMethods.Delete, new { id }),
            hyperlinkService.Create(nameof(GetInvestmentStrategiesEndpoint), RelationshipTypes.Collection, HttpMethods.Get),
            hyperlinkService.Create(nameof(AddInvestmentToStrategyEndpoint), "add-investment", HttpMethods.Post, new { id }),
            hyperlinkService.Create(nameof(UpdateInvestmentCurrentAmountsEndpoint), "update-amounts", HttpMethods.Put, new { id }),
            hyperlinkService.Create(nameof(UpdateInvestmentModelPercentagesEndpoint), "update-percentages", HttpMethods.Put, new { id })
        ];
    }
    
    public static Hyperlink[] CreateInvestmentStrategyCollectionLinks(IHyperlinkService hyperlinkService, ViaRicecoCollectionQueryParameters parameters, bool hasNextPage, bool hasPreviousPage)
    {
        List<Hyperlink> hyperlinks =
        [
            hyperlinkService.Create(nameof(GetInvestmentStrategiesEndpoint), RelationshipTypes.Self, HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, parameters.Page, parameters.PageSize
                }),
            hyperlinkService.Create(nameof(CreateInvestmentStrategyEndpoint), RelationshipTypes.Create, HttpMethods.Post)
        ];

        if (hasNextPage)
        {
            hyperlinks.Add(hyperlinkService.Create(nameof(GetInvestmentStrategiesEndpoint), RelationshipTypes.NextPage,
                HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, page = parameters.Page + 1,
                    parameters.PageSize
                }));
        }

        if (hasPreviousPage)
        {
            hyperlinks.Add(hyperlinkService.Create(nameof(GetInvestmentStrategiesEndpoint), RelationshipTypes.PreviousPage,
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
