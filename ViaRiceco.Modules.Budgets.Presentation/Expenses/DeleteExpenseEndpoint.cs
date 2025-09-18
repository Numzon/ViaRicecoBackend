using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Budgets.Application.Expenses.DeleteExpense;
using ViaRiceco.Modules.Budgets.Presentation.Enumerations;

namespace ViaRiceco.Modules.Budgets.Presentation.Expenses;

internal sealed class DeleteExpenseEndpoint(ISender sender)
    : Ep.Req<DeleteExpenseEndpoint.Request>.Res<Result>
{
    [UsedImplicitly]
    internal sealed class Request
    {
        public string Id { get; init; } = string.Empty;
    }

    public override void Configure()
    {
        Delete("/budgets/expenses/{id}");
        AllowAnonymous();
        Description(d => d.WithName(nameof(DeleteExpenseEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.Expenses)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new DeleteExpenseCommand(req.Id);
        Result result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }

        await Send.ResultAsync(Results.NoContent());
    }
}
