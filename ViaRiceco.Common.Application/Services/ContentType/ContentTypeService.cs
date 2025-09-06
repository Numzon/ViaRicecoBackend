using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace ViaRiceco.Common.Application.Services.ContentType;

public class ContentTypeService(IHttpContextAccessor httpContextAccessor) : IContentTypeService
{
    private const string HateoasContentTypePattern = "application/vnd.via-riceco.hateoas";
    private const string StandardJsonContentType = "application/json";

    public bool ShouldIncludeHateoasLinks()
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.Request.Headers.Accept.Count > 0)
        {
            return httpContext.Request.Headers.Accept
                .Any(acceptHeader => acceptHeader?.Contains(HateoasContentTypePattern, StringComparison.OrdinalIgnoreCase) == true);
        }

        return false;
    }

    public string GetResponseContentType(string? version = null)
    {
        version ??= ExtractVersionFromRequest() ?? "1.0"; 

        if (!ShouldIncludeHateoasLinks())
        {
            return $"{StandardJsonContentType};v={version}";
        }

        return $"{HateoasContentTypePattern}.v{version}+json";
    }

    private string? ExtractVersionFromRequest()
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.Request.Headers.Accept.Count > 0)
        {
            foreach (string? acceptHeader in httpContext.Request.Headers.Accept)
            {
                if (acceptHeader?.Contains(HateoasContentTypePattern, StringComparison.OrdinalIgnoreCase) == true)
                {
                    Match match = Regex.Match(acceptHeader, @"\.v(\d+(?:\.\d+)?)\+");
                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                }
                else if (acceptHeader?.Contains(StandardJsonContentType, StringComparison.OrdinalIgnoreCase) == true)
                {
                    Match match = Regex.Match(acceptHeader, @";v=(\d+(?:\.\d+)?)");
                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                }
            }
        }

        return null;
    }
}
