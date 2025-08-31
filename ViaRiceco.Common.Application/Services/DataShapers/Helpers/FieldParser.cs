using System.Collections.Concurrent;

namespace ViaRiceco.Common.Application.Services.DataShapers.Helpers;

internal static class FieldParser
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> _fieldParsingCache = new();
    
    public static HashSet<string> ParseFields(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return [];
        }

        return _fieldParsingCache.GetOrAdd(fields, static f =>
        {
            return f.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(field => field.Trim())
                .Where(field => !string.IsNullOrEmpty(field))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        });
    }

    public static bool ValidateFields<T>(HashSet<string> requestedFields, PropertyAccessor<T>[] propertyAccessors)
    {
        if (requestedFields.Count == 0)
        {
            return true;
        }

        return requestedFields.All(field => 
            propertyAccessors.Any(pa => pa.PropertyName.Equals(field, StringComparison.OrdinalIgnoreCase)));
    }
}
