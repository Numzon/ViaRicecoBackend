using FastEndpoints;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.CreateTaxType;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;

namespace ViaRiceco.Modules.Accounting.Presentation.TaxTypes;

internal sealed class CreateTaxTypeEndpoint(ISender sender) : Ep.Req<CreateTaxTypeEndpoint.Request>.Res<Result<TaxTypeDto>>
{
    [UsedImplicitly]
    internal sealed record Request(string Name);
    
    public override void Configure()
    {
        Post("/accounting/tax-types");
        Tags("TaxTypes");
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new CreateTaxTypeCommand(req.Name);
        Result<TaxTypeDto> result = await sender.Send(command, ct);

        await Send.ResultAsync(result.Match(Results.Ok, ApiResults.Problem));
    }
}
