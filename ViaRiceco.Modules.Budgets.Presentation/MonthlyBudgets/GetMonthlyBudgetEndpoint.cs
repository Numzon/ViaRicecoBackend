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
using ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.GetMonthlyBudget;
using ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.Models;
using ViaRiceco.Modules.Budgets.Presentation.Enumerations;
using ViaRiceco.Modules.Budgets.Presentation.MonthlyBudgets.Hyperlinks;

namespace ViaRiceco.Modules.Budgets.Presentation.MonthlyBudgets;

internal sealed class GetMonthlyBudgetEndpoint(ISender sender, IHyperlinkService hyperlinkService, IDataShapingService dataShapingService)
    : Ep.Req<GetMonthlyBudgetEndpoint.Request>.Res<Result<MonthlyBudgetDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public string Id { get; init; } = string.Empty;
    }

    public override void Configure()
    {
        Get("/budgets/monthly-budgets/{id}");
        AllowAnonymous();
        Description(d => d.WithName(nameof(GetMonthlyBudgetEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.MonthlyBudgets)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var query = new GetMonthlyBudgetQuery(req.Id);
        Result<MonthlyBudgetDto> result = await sender.Send(query, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        ExpandoObject shapedObject = ShapeDataWithConditionalLinks(result.Value, req.IncludeLinks, result.Value.Id);
        
        await Send.ResultAsync(Results.Ok(shapedObject));
    }

    private ExpandoObject ShapeDataWithConditionalLinks(MonthlyBudgetDto data, bool includeLinks, string monthlyBudgetId, string? fields = null)
    {
        return includeLinks
            ? dataShapingService.ShapeData(data, fields, MonthlyBudgetsHyperlinks.CreateMonthlyBudgetItemLinks(hyperlinkService, monthlyBudgetId))
            : dataShapingService.ShapeData(data, fields);
    }
}
