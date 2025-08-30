using System.Dynamic;
using FastEndpoints;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Services.DataShapers;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.CreateTaxType;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;

namespace ViaRiceco.Modules.Accounting.Presentation.TaxTypes;

internal sealed class CreateTaxTypeEndpoint(ISender sender, IHyperlinkService hyperlinkService, IDataShapingService dataShapingService)
    : Ep.Req<CreateTaxTypeEndpoint.Request>.Res<Result<TaxTypeDto>>
{
    [UsedImplicitly]
    internal sealed record Request(string Name);

    public override void Configure()
    {
        Post("/accounting/tax-types");
        Tags("TaxTypes");
        AllowAnonymous();
        Description(d => d.WithName(nameof(CreateTaxTypeEndpoint)));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new CreateTaxTypeCommand(req.Name);
        Result<TaxTypeDto> result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        Hyperlink[] links = GetLinks(result.Value.Id);
        ExpandoObject shapedObject = dataShapingService.ShapeData(result.Value, links);
        
        await Send.ResultAsync(Results.CreatedAtRoute(
            nameof(GetTaxTypeEndpoint), 
            new { id = result.Value.Id },   
            shapedObject));
    }

    private Hyperlink[] GetLinks(string id)
    {
        return
        [
            hyperlinkService.Create(nameof(GetTaxTypeEndpoint), "self", HttpMethods.Get, new { id }),
            hyperlinkService.Create(nameof(UpdateTaxTypeEndpoint), "update", HttpMethods.Put, new { id }),
            hyperlinkService.Create(nameof(DeleteTaxTypeEndpoint), "delete", HttpMethods.Delete, new { id }),
            hyperlinkService.Create(nameof(GetTaxTypesEndpoint), "collection", HttpMethods.Get)
        ];
    }
}
