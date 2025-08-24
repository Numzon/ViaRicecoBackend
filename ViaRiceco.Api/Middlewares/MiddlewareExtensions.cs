namespace ViaRiceco.Api.Middlewares;

internal static class MiddlewareExtensions
{
    internal static IApplicationBuilder UseLogContext(this IApplicationBuilder app)
    {
        app.UseMiddleware<LogContextMiddleware>();

        return app;
    }
}
