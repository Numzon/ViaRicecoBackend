using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Enumerations;

namespace ViaRiceco.Modules.Budgets.Presentation.MonthlyBudgets.Hyperlinks;

public static class MonthlyBudgetsHyperlinks
{
    public static Hyperlink[] CreateMonthlyBudgetItemLinks(IHyperlinkService hyperlinkService, string id)
    {
        return
        [
            hyperlinkService.Create(nameof(GetMonthlyBudgetEndpoint), RelationshipTypes.Self, HttpMethods.Get, new { id }),
            hyperlinkService.Create(nameof(FinalizeMonthlyBudgetEndpoint), RelationshipTypes.Finalize, HttpMethods.Post, new { id }),
            hyperlinkService.Create(nameof(SetMonthlyBudgetAsDraftEndpoint), RelationshipTypes.SetAsDraft, HttpMethods.Post, new { id })
        ];
    }
}
