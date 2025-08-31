
using System.Text;

namespace ViaRiceco.Common.Application.Services.Sorting;

public sealed record SortMapping(string SortField, string PropertyName, bool Reverse = false);

public interface ISortMappingDefinition;

#pragma warning disable S2326
public sealed class SortMappingDefinition<TSource, TDestination> : ISortMappingDefinition
#pragma warning restore S2326
{
    public required SortMapping[] Mappings { get; init; }
}

public interface ISortMappingProvider
{
    bool ValidateMappings<TSource, TDestination>(string? sort);
    SortMapping[] GetMappings<TSource, TDestination>();
}

public sealed class SortMappingProvider(IEnumerable<ISortMappingDefinition> sortMappingDefinitions) : ISortMappingProvider
{
    public SortMapping[] GetMappings<TSource, TDestination>()
    {
        SortMappingDefinition<TSource, TDestination>? definition = sortMappingDefinitions
            .OfType<SortMappingDefinition<TSource, TDestination>>()
            .FirstOrDefault();

        if (definition is null)
        {
            throw new InvalidOperationException($"No sort mapping definition found for {typeof(TSource).Name} to {typeof(TDestination).Name}");
        }
        return definition.Mappings;
    }

    public bool ValidateMappings<TSource, TDestination>(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return true;
        }

        var sortFields = sort
            .Split(',')
            .Select(s => s.Trim().Split(' ')[0])
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        SortMapping[] mappings = GetMappings<TSource, TDestination>();

        return sortFields.TrueForAll(f => mappings.Any(m => m.SortField.Equals(f, StringComparison.OrdinalIgnoreCase)));
    }
}

public sealed record SortItem(string SortField, bool IsDescending);

public static class QueryableExtensions
{
    private const string AscendingDirection = "ASC";
    private const string DescendingDirection = "DESC";
    
    public static string ApplySort(
        string? sort,
        SortMapping[] mappings,
        string defaultOrderBy = "Id")
    {
        ArgumentNullException.ThrowIfNull(mappings);

        if (string.IsNullOrWhiteSpace(sort))
        {
            return defaultOrderBy;
        }

        Dictionary<string, SortMapping> mappingLookup = CreateMappingLookup(mappings);
        
        string orderByClause = BuildOrderByClause(sort, mappingLookup);
        
        return string.IsNullOrEmpty(orderByClause) 
            ? defaultOrderBy
            : orderByClause;
    }

    private static Dictionary<string, SortMapping> CreateMappingLookup(SortMapping[] mappings)
    {
        var lookup = new Dictionary<string, SortMapping>(
            capacity: mappings.Length, 
            comparer: StringComparer.OrdinalIgnoreCase);

        foreach (SortMapping mapping in mappings)
        {
            if (string.IsNullOrWhiteSpace(mapping.SortField))
            {
                continue;
            }

            lookup.TryAdd(mapping.SortField, mapping);
        }

        return lookup;
    }
    
    private static string BuildOrderByClause(string sort, Dictionary<string, SortMapping> mappingLookup)
    {
        List<string> sortFields = ParseSortFields(sort);
        if (sortFields.Count == 0)
        {
            return string.Empty;
        }

        var validSortParts = new List<string>(capacity: sortFields.Count);

        foreach (string field in sortFields)
        {
            if (TryCreateSortPart(field, mappingLookup, out string? sortPart))
            {
                validSortParts.Add(sortPart!);
            }
        }

        return validSortParts.Count == 0 
            ? string.Empty 
            : string.Join(", ", validSortParts);
    }
    
    private static List<string> ParseSortFields(string sort)
    {
        var fields = new List<string>();
        string[] parts = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string part in parts)
        {
            string trimmed = part.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                fields.Add(trimmed);
            }
        }

        return fields;
    }
    
    private static bool TryCreateSortPart(
        string field, 
        Dictionary<string, SortMapping> mappingLookup, 
        out string? sortPart)
    {
        SortItem sortItem = ParseSortField(field);
        
        if (!mappingLookup.TryGetValue(sortItem.SortField, out SortMapping? mapping))
        {
            sortPart = null;
            return false;
        }

        string direction = DetermineDirection(sortItem.IsDescending, mapping.Reverse);
        sortPart = $"{mapping.PropertyName} {direction}";
        return true;
    }

    private static string DetermineDirection(bool isDescending, bool reverse)
    {
        return isDescending ^ reverse ? DescendingDirection : AscendingDirection;
    }
    
    private static SortItem ParseSortField(string field)
    {
        int spaceIndex = field.IndexOf(' ');
        if (spaceIndex == -1)
        {
            return new (field, false);
        }

        string sortField = field.Substring(0, spaceIndex);
        string directionPart = field.Substring(spaceIndex + 1);
        bool isDescending = directionPart.Equals(DescendingDirection.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);

        return new (sortField, isDescending);
    }
}
