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
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Portfolios.Presentation.Enumerations;
using ViaRiceco.Modules.Portfolios.Application.Currencies.CreateCurrency;
using ViaRiceco.Modules.Portfolios.Application.Currencies.Models;
using ViaRiceco.Modules.Portfolios.Presentation.Currencies.Hyperlinks;

namespace ViaRiceco.Modules.Portfolios.Presentation.Currencies;

internal sealed class CreateCurrencyEndpoint(ISender sender, IHyperlinkService hyperlinkService, IDataShapingService dataShapingService)
    : Ep.Req<CreateCurrencyEndpoint.Request>.Res<Result<CurrencyDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public string Name { get; init; }
        public string Code { get; init; }
    }

    public override void Configure()
    {
        Post("/portfolios/currencies");
        AllowAnonymous();
        Description(d => d.WithName(nameof(CreateCurrencyEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.Currencies)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new CreateCurrencyCommand(req.Name, req.Code);
        Result<CurrencyDto> result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        ExpandoObject shapedObject = ShapeDataWithConditionalLinks(result.Value, req.IncludeLinks, result.Value.Id);
        
        await Send.ResultAsync(Results.CreatedAtRoute(
            nameof(GetCurrencyEndpoint), 
            new { id = result.Value.Id },   
            shapedObject));
    }

    private ExpandoObject ShapeDataWithConditionalLinks(CurrencyDto data, bool includeLinks, string currencyId, string? fields = null)
    {
        return includeLinks
            ? dataShapingService.ShapeData(data, fields, CurrenciesHyperlinks.CreateCurrencyItemLinks(hyperlinkService, currencyId))
            : dataShapingService.ShapeData(data, fields);
    }
}
