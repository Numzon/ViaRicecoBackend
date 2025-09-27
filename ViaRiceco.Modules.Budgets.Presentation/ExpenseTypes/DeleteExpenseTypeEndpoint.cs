using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.DeleteExpenseType;
using ViaRiceco.Modules.Budgets.Presentation.Enumerations;

namespace ViaRiceco.Modules.Budgets.Presentation.ExpenseTypes;

internal sealed class DeleteExpenseTypeEndpoint(ISender sender)
    : Ep.Req<DeleteExpenseTypeEndpoint.Request>.Res<Result>
{
    [UsedImplicitly]
    internal sealed class Request
    {
        public string Id { get; init; } = string.Empty;
    }

    public override void Configure()
    {
        Delete("/budgets/expense-types/{id}");
        AllowAnonymous();
        Description(d => d.WithName(nameof(DeleteExpenseTypeEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.ExpenseTypes)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new DeleteExpenseTypeCommand(req.Id);
        Result result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }

        await Send.ResultAsync(Results.NoContent());
    }
}
