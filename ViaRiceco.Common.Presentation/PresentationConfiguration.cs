using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ViaRiceco.Common.Presentation.Enumerations;

namespace ViaRiceco.Common.Presentation;

public static class PresentationConfiguration
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, Assembly[] assemblies)
    {
        services.AddFastEndpoints(options =>
        {
            options.Assemblies = assemblies;
        });

        services.AddVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1.0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.UnsupportedApiVersionStatusCode = 406; 
            options.ReportApiVersions = true;
            options.ApiVersionSelector = new DefaultApiVersionSelector(options);
            
            options.ApiVersionReader = ApiVersionReader.Combine(
                new MediaTypeApiVersionReader(),
                new MediaTypeApiVersionReaderBuilder()
                    .Template("application/vnd.via-riceco.hateoas.{version}+json")
                    .Build());
            
        });

        VersionSets.CreateApi(CustomVersionSets.TaxTypes, v => v
            .HasApiVersion(1.0)
            .HasApiVersion(2.0));
        
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
