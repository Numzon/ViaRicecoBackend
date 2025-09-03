using MediatR;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.CreateTaxType;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;

namespace ViaRiceco.Modules.Accounting.IntegrationTests.Abstractions;

internal static class CommandHelpers
{
    internal static async Task<string> CreateTaxTypeAsync(this ISender sender, string name)
    {
        Result<TaxTypeDto> result = await sender.Send(new CreateTaxTypeCommand(name));

        return result.Value.Id;
    }
}
