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
using ViaRiceco.Common.Presentation.Enumerations;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Portfolios.Application.FinancialGoals.GetFinancialGoal;
using ViaRiceco.Modules.Portfolios.Application.FinancialGoals.Models;
using ViaRiceco.Modules.Portfolios.Presentation.Enumerations;
using ViaRiceco.Modules.Portfolios.Presentation.FinancialGoals.Hyperlinks;

namespace ViaRiceco.Modules.Portfolios.Presentation.FinancialGoals;

internal sealed class GetFinancialGoalEndpoint(ISender sender, IHyperlinkService hyperlinkService, IDataShapingService dataShapingService)
    : Ep.Req<GetFinancialGoalEndpoint.Request>.Res<Result<FinancialGoalDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public string Id { get; init; }
    }

    public override void Configure()
    {
        Get("/portfolios/financial-goals/{id}");
        Tags(EndpointTags.FinancialGoals);
        AllowAnonymous();
        Description(d => d.WithName(nameof(GetFinancialGoalEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.FinancialGoals)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var query = new GetFinancialGoalQuery(req.Id);
        Result<FinancialGoalDto> result = await sender.Send(query, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        ExpandoObject shapedObject = ShapeDataWithConditionalLinks(result.Value, req.IncludeLinks, result.Value.Id);
        
        await Send.ResultAsync(Results.Ok(shapedObject));
    }

    private ExpandoObject ShapeDataWithConditionalLinks(FinancialGoalDto data, bool includeLinks, string financialGoalId, string? fields = null)
    {
        return includeLinks
            ? dataShapingService.ShapeData(data, fields, FinancialGoalsHyperlinks.CreateFinancialGoalItemLinks(hyperlinkService, financialGoalId))
            : dataShapingService.ShapeData(data, fields);
    }
}
