using FastEndpoints;
using FastEndpoints.AspVersioning;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Presentation.Results;
using ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.RemovePurchaseRecord;
using ViaRiceco.Modules.Portfolios.Presentation.Enumerations;

namespace ViaRiceco.Modules.Portfolios.Presentation.PurchaseRecords;

internal sealed class DeletePurchaseRecordEndpoint(ISender sender)
    : Ep.Req<DeletePurchaseRecordEndpoint.Request>.Res<Result>
{
    [UsedImplicitly]
    internal sealed class Request
    {
        public string InvestmentStrategyId { get; init; }
        public string InvestmentId { get; init; }
        public string Id { get; init; }
    }

    public override void Configure()
    {
        Delete("/portfolios/investment-strategies/{investmentStrategyId}/investments/{investmentId}/purchase-records/{id}");
        AllowAnonymous();
        Description(d => d.WithName(nameof(DeletePurchaseRecordEndpoint)));
        
        Options(x => x
            .WithVersionSet(CustomVersionSets.PurchaseRecords)
            .MapToApiVersion(1.0));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var command = new RemovePurchaseRecordCommand(req.InvestmentStrategyId, req.InvestmentId, req.Id);
        Result result = await sender.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.ResultAsync(ApiResults.Problem(result));
            return;
        }
        
        await Send.ResultAsync(Results.NoContent());
    }
}
