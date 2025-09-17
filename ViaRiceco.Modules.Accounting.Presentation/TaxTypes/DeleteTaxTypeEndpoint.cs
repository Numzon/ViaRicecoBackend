using System.Dynamic;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Services.DataShapers;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.DeleteTaxType;
using ViaRiceco.Modules.Accounting.Presentation.Enumerations;

namespace ViaRiceco.Modules.Accounting.Presentation.TaxTypes;

internal sealed class DeleteTaxTypeEndpoint(ISender sender)
    : Ep.Req<DeleteTaxTypeEndpoint.Request>.Res<Result>
{
    [UsedImplicitly]
    internal sealed record Request(string Id);

    public override void Configure()
    {
        Delete("/accounting/tax-types/{id}");
        AllowAnonymous();
        Description(d => d.WithName(nameof(DeleteTaxTypeEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.TaxTypes)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new DeleteTaxTypeCommand(req.Id);
        Result result = await sender.Send(command, ct);

        await Send.ResultAsync(result.Match(Results.NoContent, ApiResults.Problem));
    }
}
