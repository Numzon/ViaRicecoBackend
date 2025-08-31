using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ViaRiceco.Common.Application.Services.Sorting.Extensions;

public static class SortMappingRegistrationExtensions
{
    public static IServiceCollection AddSortMappings(this IServiceCollection services, params Assembly[] assemblies)
    {
        foreach (Assembly assembly in assemblies)
        {
            var sourceTypes = assembly.GetTypes()
                .Where(type => typeof(ISortMappingSource).IsAssignableFrom(type) 
                              && type is { IsClass: true, IsAbstract: false })
                .ToList();

            foreach (Type sourceType in sourceTypes)
            {
                if (Activator.CreateInstance(sourceType) is ISortMappingSource source)
                {
                    ISortMappingDefinition mappingDefinition = source.GetSortMappingDefinition();
                    services.AddSingleton(mappingDefinition);
                }
            }
        }

        return services;
    }
}
