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
using ViaRiceco.Modules.Budgets.Application.Banks.CreateBank;
using ViaRiceco.Modules.Budgets.Application.Banks.Models;
using ViaRiceco.Modules.Budgets.Presentation.Banks.Hyperlinks;

namespace ViaRiceco.Modules.Budgets.Presentation.Banks;

internal sealed class CreateBankEndpoint(ISender sender, IHyperlinkService hyperlinkService, IDataShapingService dataShapingService)
    : Ep.Req<CreateBankEndpoint.Request>.Res<Result<BankDto>>
{
    [UsedImplicitly]
    internal sealed class Request : BaseAcceptHeader
    {
        public string Name { get; init; } = string.Empty;
    }

    public override void Configure()
    {
        Post("/budgets/banks");
        AllowAnonymous();
        Description(d => d.WithName(nameof(CreateBankEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.Banks)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new CreateBankCommand(req.Name);
        Result<BankDto> result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        ExpandoObject shapedObject = ShapeDataWithConditionalLinks(result.Value, req.IncludeLinks, result.Value.Id);
        
        await Send.ResultAsync(Results.CreatedAtRoute(
            nameof(GetBankEndpoint), 
            new { id = result.Value.Id },   
            shapedObject));
    }

    private ExpandoObject ShapeDataWithConditionalLinks(BankDto data, bool includeLinks, string bankId, string? fields = null)
    {
        return includeLinks
            ? dataShapingService.ShapeData(data, fields, BanksHyperlinks.CreateBankItemLinks(hyperlinkService, bankId))
            : dataShapingService.ShapeData(data, fields);
    }
}
