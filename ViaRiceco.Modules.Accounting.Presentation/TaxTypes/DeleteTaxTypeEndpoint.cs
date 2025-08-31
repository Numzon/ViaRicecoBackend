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
using ViaRiceco.Modules.Accounting.Application.TaxTypes.DeleteTaxType;

namespace ViaRiceco.Modules.Accounting.Presentation.TaxTypes;

internal sealed class DeleteTaxTypeEndpoint(ISender sender)
    : Ep.Req<DeleteTaxTypeEndpoint.Request>.Res<Result>
{
    [UsedImplicitly]
    internal sealed record Request(string Id);

    public override void Configure()
    {
        Delete("/accounting/tax-types/{id}");
        Tags("TaxTypes");
        AllowAnonymous();
        Description(d => d.WithName(nameof(DeleteTaxTypeEndpoint)));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new DeleteTaxTypeCommand(req.Id);
        Result result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        await Send.ResultAsync(Results.NoContent());
    }
}
