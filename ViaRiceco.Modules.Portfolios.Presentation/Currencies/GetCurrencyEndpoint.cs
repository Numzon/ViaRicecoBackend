using System.Dynamic;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Services.DataShapers;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Abstractions.Headers;
using ViaRiceco.Common.Presentation.Enumerations;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Portfolios.Application.Currencies.GetCurrency;
using ViaRiceco.Modules.Portfolios.Application.Currencies.Models;
using ViaRiceco.Modules.Portfolios.Presentation.Enumerations;
using ViaRiceco.Modules.Portfolios.Presentation.Currencies.Hyperlinks;

namespace ViaRiceco.Modules.Portfolios.Presentation.Currencies;

internal sealed class GetCurrencyEndpoint(ISender sender, IHyperlinkService hyperlinkService, IDataShapingService dataShapingService)
    : Ep.Req<GetCurrencyEndpoint.Request>.Res<Result<CurrencyDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public string Id { get; init; }
    }

    public override void Configure()
    {
        Get("/portfolios/currencies/{id}");
        Tags(EndpointTags.Currencies);
        AllowAnonymous();
        Description(d => d.WithName(nameof(GetCurrencyEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.Currencies)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var query = new GetCurrencyQuery(req.Id);
        Result<CurrencyDto> result = await sender.Send(query, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        ExpandoObject shapedObject = ShapeDataWithConditionalLinks(result.Value, req.IncludeLinks, result.Value.Id);
        
        await Send.ResultAsync(Results.Ok(shapedObject));
    }

    private ExpandoObject ShapeDataWithConditionalLinks(CurrencyDto data, bool includeLinks, string currencyId, string? fields = null)
    {
        return includeLinks
            ? dataShapingService.ShapeData(data, fields, CurrenciesHyperlinks.CreateCurrencyItemLinks(hyperlinkService, currencyId))
            : dataShapingService.ShapeData(data, fields);
    }
}
