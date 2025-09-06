using MediatR;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.CreateTaxType;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.DeleteTaxType;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.UpdateTaxType;

namespace ViaRiceco.Modules.Accounting.IntegrationTests.Abstractions;

internal static class CommandHelpers
{
    internal static async Task<string> CreateTaxTypeAsync(this ISender sender, string name)
    {
        Result<TaxTypeDto> result = await sender.Send(new CreateTaxTypeCommand(name));

        return result.Value.Id;
    }

    internal static async Task<TaxTypeDto> UpdateTaxTypeAsync(this ISender sender, string id, string name)
    {
        Result<TaxTypeDto> result = await sender.Send(new UpdateTaxTypeCommand(id, name));

        return result.Value;
    }

    internal static async Task DeleteTaxTypeAsync(this ISender sender, string id)
    {
        await sender.Send(new DeleteTaxTypeCommand(id));
    }

    internal static async Task<List<string>> CreateMultipleTaxTypesAsync(this ISender sender, params string[] names)
    {
        List<string> ids = [];

        foreach (string name in names)
        {
            string id = await sender.CreateTaxTypeAsync(name);
            ids.Add(id);
        }

        return ids;
    }

    internal static async Task<List<string>> CreateMultipleTaxTypesAsync(this ISender sender, int count, string namePrefix = "Tax Type")
    {
        List<string> ids = [];

        for (int i = 1; i <= count; i++)
        {
            string name = $"{namePrefix} {i:D2}";
            string id = await sender.CreateTaxTypeAsync(name);
            ids.Add(id);
        }

        return ids;
    }
}
