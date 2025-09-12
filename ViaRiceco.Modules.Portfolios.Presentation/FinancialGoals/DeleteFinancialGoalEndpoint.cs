using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Enumerations;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Portfolios.Application.FinancialGoals.DeleteFinancialGoal;
using ViaRiceco.Modules.Portfolios.Presentation.Enumerations;

namespace ViaRiceco.Modules.Portfolios.Presentation.FinancialGoals;

internal sealed class DeleteFinancialGoalEndpoint(ISender sender)
    : Ep.Req<DeleteFinancialGoalEndpoint.Request>.Res<Result>
{
    [UsedImplicitly]
    internal sealed class Request
    {
        public string Id { get; init; }
    }

    public override void Configure()
    {
        Delete("/portfolios/financial-goals/{id}");
        Tags(EndpointTags.FinancialGoals);
        AllowAnonymous();
        Description(d => d.WithName(nameof(DeleteFinancialGoalEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.FinancialGoals)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new DeleteFinancialGoalCommand(req.Id);
        Result result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }

        await Send.ResultAsync(Results.NoContent());
    }
}
