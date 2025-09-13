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
using ViaRiceco.Modules.Accounting.Presentation.Enumerations;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.CreateTaxType;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Accounting.Presentation.TaxTypes.Hyperlinks;

namespace ViaRiceco.Modules.Accounting.Presentation.TaxTypes;

internal sealed class CreateTaxTypeEndpoint(ISender sender, IHyperlinkService hyperlinkService, IDataShapingService dataShapingService)
    : Ep.Req<CreateTaxTypeEndpoint.Request>.Res<Result<TaxTypeDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public string Name { get; init; }
    }

    public override void Configure()
    {
        Post("/accounting/tax-types");
        AllowAnonymous();
        Description(d => d.WithName(nameof(CreateTaxTypeEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.TaxTypes)
            .MapToApiVersion(1.0));
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
        
        ExpandoObject shapedObject = ShapeDataWithConditionalLinks(result.Value, req.IncludeLinks, result.Value.Id);
        
        await Send.ResultAsync(Results.CreatedAtRoute(
            nameof(GetTaxTypeEndpoint), 
            new { id = result.Value.Id },   
            shapedObject));
    }

    private ExpandoObject ShapeDataWithConditionalLinks(TaxTypeDto data, bool includeLinks, string taxTypeId, string? fields = null)
    {
        return includeLinks
            ? dataShapingService.ShapeData(data, fields, TaxTypesHyperlinks.CreateTaxTypeItemLinks(hyperlinkService, taxTypeId))
            : dataShapingService.ShapeData(data, fields);
    }
}
