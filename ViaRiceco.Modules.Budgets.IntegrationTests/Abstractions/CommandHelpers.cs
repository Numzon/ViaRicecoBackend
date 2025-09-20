using System.Text;
using System.Text.Json;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

internal static class CommandHelpers
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    internal static StringContent SerializeToStringContent<T>(T obj)
    {
        string json = JsonSerializer.Serialize(obj);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    internal static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions)!;
    }
}
