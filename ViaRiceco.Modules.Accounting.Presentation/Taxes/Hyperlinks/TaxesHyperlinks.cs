using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Modules.Accounting.Presentation.SettlementPeriods;

namespace ViaRiceco.Modules.Accounting.Presentation.Taxes.Hyperlinks;

public static class TaxesHyperlinks
{
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

