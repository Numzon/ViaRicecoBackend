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
using ViaRiceco.Modules.Portfolios.Application.FinancialGoals.CreateFinancialGoal;
using ViaRiceco.Modules.Portfolios.Application.FinancialGoals.Models;
using ViaRiceco.Modules.Portfolios.Presentation.Enumerations;
using ViaRiceco.Modules.Portfolios.Presentation.FinancialGoals.Hyperlinks;

namespace ViaRiceco.Modules.Portfolios.Presentation.FinancialGoals;

internal sealed class CreateFinancialGoalEndpoint(ISender sender, IHyperlinkService hyperlinkService, IDataShapingService dataShapingService)
    : Ep.Req<CreateFinancialGoalEndpoint.Request>.Res<Result<FinancialGoalDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public string Name { get; init; }
        public string? ParentId { get; init; }
    }

    public override void Configure()
    {
        Post("/portfolios/financial-goals");
        Tags(EndpointTags.FinancialGoals);
        AllowAnonymous();
        Description(d => d.WithName(nameof(CreateFinancialGoalEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.FinancialGoals)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new CreateFinancialGoalCommand(req.Name, req.ParentId);
        Result<FinancialGoalDto> result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        ExpandoObject shapedObject = ShapeDataWithConditionalLinks(result.Value, req.IncludeLinks, result.Value.Id);
        
        await Send.ResultAsync(Results.CreatedAtRoute(
            nameof(GetFinancialGoalEndpoint), 
            new { id = result.Value.Id },   
            shapedObject));
    }

    private ExpandoObject ShapeDataWithConditionalLinks(FinancialGoalDto data, bool includeLinks, string financialGoalId, string? fields = null)
    {
        return includeLinks
            ? dataShapingService.ShapeData(data, fields, FinancialGoalsHyperlinks.CreateFinancialGoalItemLinks(hyperlinkService, financialGoalId))
            : dataShapingService.ShapeData(data, fields);
    }
}
