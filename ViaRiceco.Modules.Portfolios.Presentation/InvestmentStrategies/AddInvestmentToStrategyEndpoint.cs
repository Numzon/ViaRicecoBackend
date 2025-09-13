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
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.AddInvestmentToStrategy;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.Models;
using ViaRiceco.Modules.Portfolios.Presentation.Enumerations;
using ViaRiceco.Modules.Portfolios.Presentation.InvestmentStrategies.Hyperlinks;

namespace ViaRiceco.Modules.Portfolios.Presentation.InvestmentStrategies;

internal sealed class AddInvestmentToStrategyEndpoint(ISender sender, IHyperlinkService hyperlinkService, IDataShapingService dataShapingService)
    : Ep.Req<AddInvestmentToStrategyEndpoint.Request>.Res<Result<InvestmentStrategyDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public string Id { get; init; }
        public string InvestmentName { get; init; }
    }

    public override void Configure()
    {
        Post("/portfolios/investment-strategies/{id}/investments");
        AllowAnonymous();
        Description(d => d.WithName(nameof(AddInvestmentToStrategyEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.InvestmentStrategies)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new AddInvestmentToStrategyCommand(req.Id, req.InvestmentName);
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
