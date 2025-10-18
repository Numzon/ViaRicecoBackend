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
using ViaRiceco.Modules.Portfolios.Application.Investments.RemoveInvestment;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.Models;
using ViaRiceco.Modules.Portfolios.Presentation.Enumerations;
using ViaRiceco.Modules.Portfolios.Presentation.InvestmentStrategies.Hyperlinks;

namespace ViaRiceco.Modules.Portfolios.Presentation.Investments;

internal sealed class RemoveInvestmentFromStrategyEndpoint(ISender sender, IHyperlinkService hyperlinkService, IDataShapingService dataShapingService)
    : Ep.Req<RemoveInvestmentFromStrategyEndpoint.Request>.Res<Result<InvestmentStrategyDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public string Id { get; init; }
        public string InvestmentId { get; init; }
    }

    public override void Configure()
    {
        Delete("/portfolios/investment-strategies/{id}/investments/{investmentId}");
        AllowAnonymous();
        Description(d => d.WithName(nameof(RemoveInvestmentFromStrategyEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.Investments)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new RemoveInvestmentCommand(req.Id, req.InvestmentId);
        Result<InvestmentStrategyDto> result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        ExpandoObject shapedObject = ShapeDataWithConditionalLinks(result.Value, req.IncludeLinks, result.Value.Id);
        
        await Send.ResultAsync(Results.Ok(shapedObject));
    }

    private ExpandoObject ShapeDataWithConditionalLinks(InvestmentStrategyDto data, bool includeLinks, string investmentStrategyId, string? fields = null)
    {
        return includeLinks
            ? dataShapingService.ShapeData(data, fields, InvestmentStrategiesHyperlinks.CreateInvestmentStrategyItemLinks(hyperlinkService, investmentStrategyId))
            : dataShapingService.ShapeData(data, fields);
    }
}
