using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ViaRiceco.Common.Application.Services.DataShapers;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Abstractions.Headers;
using ViaRiceco.Common.Presentation.Enumerations;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.UpdateTaxType;
using ViaRiceco.Modules.Accounting.Presentation.Enumerations;
using ViaRiceco.Modules.Accounting.Presentation.TaxTypes.Hyperlinks;

namespace ViaRiceco.Modules.Accounting.Presentation.TaxTypes;

internal sealed class UpdateTaxTypeEndpoint(ISender sender, IHyperlinkService hyperlinkService, IDataShapingService dataShapingService)
    : Ep.Req<UpdateTaxTypeEndpoint.Request>.Res<Result<TaxTypeDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public string Id { get; init; }
        public string Name { get; init; }
    }

    public override void Configure()
    {
        Put("/accounting/tax-types/{id}");
        Tags(EndpointTags.TaxTypes);
        AllowAnonymous();
        Description(d => d.WithName(nameof(UpdateTaxTypeEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.TaxTypes)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new UpdateTaxTypeCommand(req.Id, req.Name);
        Result<TaxTypeDto> result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        ExpandoObject shapedObject = ShapeDataWithConditionalLinks(result.Value, req.IncludeLinks, result.Value.Id);
        
        await Send.ResultAsync(Results.Ok(shapedObject));
    }

    private ExpandoObject ShapeDataWithConditionalLinks(TaxTypeDto data, bool includeLinks, string taxTypeId, string? fields = null)
    {
        return includeLinks
            ? dataShapingService.ShapeData(data, fields, TaxTypesHyperlinks.CreateTaxTypeItemLinks(hyperlinkService, taxTypeId))
            : dataShapingService.ShapeData(data, fields);
    }
}
