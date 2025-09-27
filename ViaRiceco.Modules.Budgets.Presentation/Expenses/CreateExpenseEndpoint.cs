using System.Dynamic;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Application.Services.DataShapers;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Abstractions.Headers;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Budgets.Presentation.Enumerations;
using ViaRiceco.Modules.Budgets.Application.Expenses.CreateExpense;
using ViaRiceco.Modules.Budgets.Application.Expenses.Models;
using ViaRiceco.Modules.Budgets.Presentation.Expenses.Hyperlinks;

namespace ViaRiceco.Modules.Budgets.Presentation.Expenses;

internal sealed class CreateExpenseEndpoint(ISender sender, IHyperlinkService hyperlinkService, IDataShapingService dataShapingService)
    : Ep.Req<CreateExpenseEndpoint.Request>.Res<Result<ExpenseDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public string Name { get; init; } = string.Empty;
        public string ExpenseTypeId { get; init; } = string.Empty;
    }

    public override void Configure()
    {
        Post("/budgets/expenses");
        AllowAnonymous();
        Description(d => d.WithName(nameof(CreateExpenseEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.Expenses)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new CreateExpenseCommand(req.Name, req.ExpenseTypeId);
        Result<ExpenseDto> result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        ExpandoObject shapedObject = ShapeDataWithConditionalLinks(result.Value, req.IncludeLinks, result.Value.Id);
        
        await Send.ResultAsync(Results.CreatedAtRoute(
            nameof(GetExpenseEndpoint), 
            new { id = result.Value.Id },   
            shapedObject));
    }

    private ExpandoObject ShapeDataWithConditionalLinks(ExpenseDto data, bool includeLinks, string expenseId, string? fields = null)
    {
        return includeLinks
            ? dataShapingService.ShapeData(data, fields, ExpensesHyperlinks.CreateExpenseItemLinks(hyperlinkService, expenseId))
            : dataShapingService.ShapeData(data, fields);
    }
}
