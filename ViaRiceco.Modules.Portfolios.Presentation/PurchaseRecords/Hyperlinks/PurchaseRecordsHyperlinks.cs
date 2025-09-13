using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Presentation.Abstractions.Collections;

namespace ViaRiceco.Modules.Portfolios.Presentation.PurchaseRecords.Hyperlinks;

public static class PurchaseRecordsHyperlinks
{
    public static Hyperlink[] CreatePurchaseRecordItemLinks(IHyperlinkService hyperlinkService, string id)
    {
        return
        [
            hyperlinkService.Create(nameof(GetPurchaseRecordEndpoint), RelationshipTypes.Self, HttpMethods.Get, new { id }),
            hyperlinkService.Create(nameof(UpdatePurchaseRecordEndpoint), RelationshipTypes.Update, HttpMethods.Put, new { id }),
            hyperlinkService.Create(nameof(DeletePurchaseRecordEndpoint), RelationshipTypes.Delete, HttpMethods.Delete, new { id }),
            hyperlinkService.Create(nameof(GetPurchaseRecordsEndpoint), RelationshipTypes.Collection, HttpMethods.Get)
        ];
    }
    
    public static Hyperlink[] CreatePurchaseRecordCollectionLinks(IHyperlinkService hyperlinkService, ViaRicecoCollectionQueryParameters parameters, bool hasNextPage, bool hasPreviousPage)
    {
        List<Hyperlink> hyperlinks =
        [
            hyperlinkService.Create(nameof(GetPurchaseRecordsEndpoint), RelationshipTypes.Self, HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, parameters.Page, parameters.PageSize
                })
        ];

        if (hasNextPage)
        {
            hyperlinks.Add(hyperlinkService.Create(nameof(GetPurchaseRecordsEndpoint), RelationshipTypes.NextPage,
                HttpMethods.Get,
                new
                {
                    q = parameters.Search, parameters.Fields, parameters.Sort, page = parameters.Page + 1,
                    parameters.PageSize
                }));
        }

        if (hasPreviousPage)
        {
            hyperlinks.Add(hyperlinkService.Create(nameof(GetPurchaseRecordsEndpoint), RelationshipTypes.PreviousPage,
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
