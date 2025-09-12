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
using ViaRiceco.Modules.Portfolios.Application.InvestmentTypes.GetInvestmentStrategyType;
using ViaRiceco.Modules.Portfolios.Application.InvestmentTypes.Models;
using ViaRiceco.Modules.Portfolios.Presentation.Enumerations;
using ViaRiceco.Modules.Portfolios.Presentation.InvestmentTypes.Hyperlinks;

namespace ViaRiceco.Modules.Portfolios.Presentation.InvestmentTypes;

internal sealed class GetInvestmentTypeEndpoint(ISender sender, IHyperlinkService hyperlinkService, IDataShapingService dataShapingService)
    : Ep.Req<GetInvestmentTypeEndpoint.Request>.Res<Result<InvestmentStrategyTypeDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public string Id { get; init; }
    }

    public override void Configure()
    {
        Get("/portfolios/investment-types/{id}");
        Tags(EndpointTags.InvestmentTypes);
        AllowAnonymous();
        Description(d => d.WithName(nameof(GetInvestmentTypeEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.InvestmentTypes)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var query = new GetInvestmentStrategyTypeQuery(req.Id);
        Result<InvestmentStrategyTypeDto> result = await sender.Send(query, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        ExpandoObject shapedObject = ShapeDataWithConditionalLinks(result.Value, req.IncludeLinks, result.Value.Id);
        
        await Send.ResultAsync(Results.Ok(shapedObject));
    }

    private ExpandoObject ShapeDataWithConditionalLinks(InvestmentStrategyTypeDto data, bool includeLinks, string investmentTypeId, string? fields = null)
    {
        return includeLinks
            ? dataShapingService.ShapeData(data, fields, InvestmentTypesHyperlinks.CreateInvestmentTypeItemLinks(hyperlinkService, investmentTypeId))
            : dataShapingService.ShapeData(data, fields);
    }
}
