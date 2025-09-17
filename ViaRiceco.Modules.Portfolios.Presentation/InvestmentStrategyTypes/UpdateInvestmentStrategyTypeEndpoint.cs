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
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategyTypes.UpdateInvestmentStrategyType;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategyTypes.Models;
using ViaRiceco.Modules.Portfolios.Presentation.Enumerations;
using ViaRiceco.Modules.Portfolios.Presentation.InvestmentStrategyTypes.Hyperlinks;

namespace ViaRiceco.Modules.Portfolios.Presentation.InvestmentStrategyTypes;

internal sealed class UpdateInvestmentStrategyTypeEndpoint(ISender sender, IHyperlinkService hyperlinkService, IDataShapingService dataShapingService)
    : Ep.Req<UpdateInvestmentStrategyTypeEndpoint.Request>.Res<Result<InvestmentStrategyTypeDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public string Id { get; init; }
        public string Name { get; init; }
    }

    public override void Configure()
    {
        Put("/portfolios/investment-strategy-types/{id}");
        AllowAnonymous();
        Description(d => d.WithName(nameof(UpdateInvestmentStrategyTypeEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.InvestmentStrategyTypes)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new UpdateInvestmentStrategyTypeCommand(req.Id, req.Name);
        Result<InvestmentStrategyTypeDto> result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        ExpandoObject shapedObject = ShapeDataWithConditionalLinks(result.Value, req.IncludeLinks, result.Value.Id);
        
        await Send.ResultAsync(Results.Ok(shapedObject));
    }

    private ExpandoObject ShapeDataWithConditionalLinks(InvestmentStrategyTypeDto data, bool includeLinks, string investmentStrategyTypeId, string? fields = null)
    {
        return includeLinks
            ? dataShapingService.ShapeData(data, fields, InvestmentStrategyTypesHyperlinks.CreateInvestmentStrategyTypeItemLinks(hyperlinkService, investmentStrategyTypeId))
            : dataShapingService.ShapeData(data, fields);
    }
}
