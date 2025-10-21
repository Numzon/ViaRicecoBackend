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
using ViaRiceco.Modules.Budgets.Application.Banks.GetBank;
using ViaRiceco.Modules.Budgets.Application.Banks.Models;
using ViaRiceco.Modules.Budgets.Presentation.Banks.Hyperlinks;

namespace ViaRiceco.Modules.Budgets.Presentation.Banks;

internal sealed class GetBankEndpoint(ISender sender, IHyperlinkService hyperlinkService, IDataShapingService dataShapingService)
    : Ep.Req<GetBankEndpoint.Request>.Res<Result<BankDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public string Id { get; init; }
        public string? Fields { get; init; }
    }

    public override void Configure()
    {
        Get("/budgets/banks/{id}");
        AllowAnonymous();
        Description(d => d.WithName(nameof(GetBankEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.Banks)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var query = new GetBankQuery(req.Id);
        Result<BankDto> result = await sender.Send(query, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        ExpandoObject shapedObject = ShapeDataWithConditionalLinks(result.Value, req.IncludeLinks, result.Value.Id, req.Fields);
        
        await Send.ResultAsync(Results.Ok(shapedObject));
    }

    private ExpandoObject ShapeDataWithConditionalLinks(BankDto data, bool includeLinks, string bankId, string? fields = null)
    {
        return includeLinks
            ? dataShapingService.ShapeData(data, fields, BanksHyperlinks.CreateBankItemLinks(hyperlinkService, bankId))
            : dataShapingService.ShapeData(data, fields);
    }
}
