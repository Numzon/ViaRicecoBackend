using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Presentation.Abstractions.Collections;
using ViaRiceco.Modules.Accounting.Presentation.Incomes;

namespace ViaRiceco.Modules.Accounting.Presentation.SettlementPeriods.Hyperlinks;

public static class SettlementPeriodsHyperlinks
{
    public static Hyperlink[] CreateSettlementPeriodItemLinks(IHyperlinkService hyperlinkService, string id)
    {
        return
        [
            hyperlinkService.Create(nameof(GetSettlementPeriodEndpoint), RelationshipTypes.Self, HttpMethods.Get, new { id }),
            hyperlinkService.Create(nameof(DeleteSettlementPeriodEndpoint), RelationshipTypes.Delete, HttpMethods.Delete, new { id }),
            hyperlinkService.Create(nameof(GetSettlementPeriodsEndpoint), RelationshipTypes.Collection, HttpMethods.Get),
            hyperlinkService.Create(nameof(FinalizeSettlementPeriodEndpoint), RelationshipTypes.Finalize, HttpMethods.Post, new { id }),
            hyperlinkService.Create(nameof(SetSettlementPeriodAsDraftEndpoint), RelationshipTypes.SetAsDraft, HttpMethods.Post, new { id }),
            hyperlinkService.Create(nameof(AddIncomeEndpoint), "add-income", HttpMethods.Post, new { settlementPeriodId = id }),
            hyperlinkService.Create(nameof(AddTaxEndpoint), "add-tax", HttpMethods.Post, new { settlementPeriodId = id })
        ];
    }
    
    public static Hyperlink[] CreateSettlementPeriodCollectionLinks(IHyperlinkService hyperlinkService, SettlementPeriodCollectionQueryParameters parameters, bool hasNextPage, bool hasPreviousPage)
    {
        List<Hyperlink> hyperlinks =
        [
            hyperlinkService.Create(nameof(GetSettlementPeriodsEndpoint), RelationshipTypes.Self, HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, parameters.Page, parameters.PageSize, parameters.Month, parameters.Year
                }),
            hyperlinkService.Create(nameof(CreateSettlementPeriodEndpoint), RelationshipTypes.Create, HttpMethods.Post)
        ];

        if (hasNextPage)
        {
            hyperlinks.Add(hyperlinkService.Create(nameof(GetSettlementPeriodsEndpoint), RelationshipTypes.NextPage, HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, Page = parameters.Page + 1, parameters.PageSize, parameters.Month, parameters.Year
                }));
        }

        if (hasPreviousPage)
        {
            hyperlinks.Add(hyperlinkService.Create(nameof(GetSettlementPeriodsEndpoint), RelationshipTypes.PreviousPage, HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, Page = parameters.Page - 1, parameters.PageSize, parameters.Month, parameters.Year
                }));
        }

        return hyperlinks.ToArray();
    }

    public static Hyperlink[] CreateTaxItemLinks(IHyperlinkService hyperlinkService, string settlementPeriodId, string taxId)
    {
        return
        [
            hyperlinkService.Create(nameof(UpdateTaxEndpoint), RelationshipTypes.Update, HttpMethods.Put, new { settlementPeriodId, taxId }),
            hyperlinkService.Create(nameof(RemoveTaxEndpoint), RelationshipTypes.Delete, HttpMethods.Delete, new { settlementPeriodId, taxId }),
            hyperlinkService.Create(nameof(GetSettlementPeriodEndpoint), RelationshipTypes.Parent, HttpMethods.Get, new { id = settlementPeriodId })
        ];
    }
}
