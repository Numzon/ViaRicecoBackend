namespace ViaRiceco.Api.Extensions;

internal static class ConfigurationExtensions
{
    internal static IConfigurationBuilder AddModuleConfiguration(this IConfigurationBuilder builder, string[] modules)
    {
        foreach (string module in modules)
        {
            builder.AddJsonFile($"modules.{module}.json", optional: false, reloadOnChange: true);
            builder.AddJsonFile($"modules.{module}.Development.json", optional: false, reloadOnChange: true);
        }

        return builder;
    }
}
