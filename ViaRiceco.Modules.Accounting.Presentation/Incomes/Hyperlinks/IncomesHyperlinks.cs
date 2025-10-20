using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Modules.Accounting.Presentation.SettlementPeriods;

namespace ViaRiceco.Modules.Accounting.Presentation.Incomes.Hyperlinks;

public static class IncomesHyperlinks
{
    public static Hyperlink[] CreateIncomeItemLinks(IHyperlinkService hyperlinkService, string settlementPeriodId, string incomeId)
    {
        return
        [
            hyperlinkService.Create(nameof(UpdateIncomeEndpoint), RelationshipTypes.Update, HttpMethods.Put, new { settlementPeriodId, incomeId }),
            hyperlinkService.Create(nameof(RemoveIncomeEndpoint), RelationshipTypes.Delete, HttpMethods.Delete, new { settlementPeriodId, incomeId }),
            hyperlinkService.Create(nameof(GetSettlementPeriodEndpoint), RelationshipTypes.Parent, HttpMethods.Get, new { id = settlementPeriodId })
        ];
    }
}

