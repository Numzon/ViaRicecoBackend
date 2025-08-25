namespace ViaRiceco.Common.Application.Extensions;

public static class TimeProviderExtensions
{
    public static DateTime UtcNow(this TimeProvider timeProvider)
    {
        return timeProvider.GetUtcNow().UtcDateTime;
    }
}
