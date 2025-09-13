using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Portfolios.Application.Currencies.DeleteCurrency;
using ViaRiceco.Modules.Portfolios.Presentation.Enumerations;

namespace ViaRiceco.Modules.Portfolios.Presentation.Currencies;

internal sealed class DeleteCurrencyEndpoint(ISender sender)
    : Ep.Req<DeleteCurrencyEndpoint.Request>.Res<Result>
{
    [UsedImplicitly]
    internal sealed class Request
    {
        public string Id { get; init; }
    }

    public override void Configure()
    {
        Delete("/portfolios/currencies/{id}");
        AllowAnonymous();
        Description(d => d.WithName(nameof(DeleteCurrencyEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.Currencies)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new DeleteCurrencyCommand(req.Id);
        Result result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }

        await Send.ResultAsync(Results.NoContent());
    }
}
