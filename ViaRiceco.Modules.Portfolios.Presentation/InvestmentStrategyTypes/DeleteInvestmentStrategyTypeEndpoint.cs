using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategyTypes.DeleteInvestmentStrategyType;
using ViaRiceco.Modules.Portfolios.Presentation.Enumerations;

namespace ViaRiceco.Modules.Portfolios.Presentation.InvestmentStrategyTypes;

internal sealed class DeleteInvestmentStrategyTypeEndpoint(ISender sender)
    : Ep.Req<DeleteInvestmentStrategyTypeEndpoint.Request>.Res<Result>
{
    [UsedImplicitly]
    internal sealed class Request
    {
        public string Id { get; init; }
    }

    public override void Configure()
    {
        Delete("/portfolios/investment-strategy-types/{id}");
        AllowAnonymous();
        Description(d => d.WithName(nameof(DeleteInvestmentStrategyTypeEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.InvestmentStrategyTypes)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new DeleteInvestmentStrategyTypeCommand(req.Id);
        Result result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }

        await Send.ResultAsync(Results.NoContent());
    }
}
