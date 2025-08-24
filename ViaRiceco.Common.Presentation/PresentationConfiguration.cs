using System.Reflection;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ViaRiceco.Common.Presentation;

public static class PresentationConfiguration
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, Assembly[] assemblies)
    {
        services.AddFastEndpoints(options =>
        {
            options.Assemblies = assemblies;
        });
        
        return services;
    }

    public static WebApplication MapFastEndpoints(this WebApplication app)
    {
        app.UseFastEndpoints(c =>
        {
            c.Endpoints.RoutePrefix = "api";
        });

        return app;
    }
}
