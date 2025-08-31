using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ViaRiceco.Common.Application.Behaviors;
using ViaRiceco.Common.Application.Services;
using ViaRiceco.Common.Application.Services.DataShapers;
using ViaRiceco.Common.Application.Services.Hyperlinks;
using ViaRiceco.Common.Application.Services.Sorting;

namespace ViaRiceco.Common.Application;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplication(this IServiceCollection services, Assembly[] assemblies)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(assemblies);
            
            config.AddOpenBehavior(typeof(ExceptionHandlingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssemblies(assemblies);
        
        services.AddTransient<IDataShapingService, DataShapingService>();
        services.AddTransient<IHyperlinkService, HyperlinkService>();
        services.AddTransient<ISortMappingProvider, SortMappingProvider>();

        return services;
    }
}
