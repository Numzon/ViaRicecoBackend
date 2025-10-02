using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Budgets.Presentation.Enumerations;
using ViaRiceco.Modules.Budgets.Application.Banks.DeleteBank;

namespace ViaRiceco.Modules.Budgets.Presentation.Banks;

internal sealed class DeleteBankEndpoint(ISender sender) : Ep.Req<DeleteBankEndpoint.Request>.Res<Result>
{
    [UsedImplicitly]
    internal sealed class Request
    {
        public string Id { get; init; } = string.Empty;
    }

    public override void Configure()
    {
        Delete("/budgets/banks/{id}");
        AllowAnonymous();
        Description(d => d.WithName(nameof(DeleteBankEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.Banks)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new DeleteBankCommand(req.Id);
        Result result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }

        await Send.ResultAsync(Results.NoContent());
    }
}
