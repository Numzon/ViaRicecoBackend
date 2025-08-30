using System.Dynamic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Common.Application.Services.Hyperlinks;

public class HyperlinkService(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator) 
    : IHyperlinkService
{
    public Hyperlink Create(string endpointName, string rel, string method, object? values = null)
    {
        HttpContext httpContext = httpContextAccessor.HttpContext 
            ?? throw new InvalidOperationException("HttpContext is not available");

        // Generate absolute URL
        string? url = linkGenerator.GetUriByName(httpContext, endpointName, values);
        
        return new Hyperlink
        {
            Href = url ?? throw new InvalidOperationException($"Could not generate URL for endpoint '{endpointName}'"),
            Rel = rel,
            Method = method
        };
    }

    public Result<ExpandoObject> AddHyperlinks(Result<ExpandoObject> result, Hyperlink[] hyperlinks)
    {
        if (!result.IsSuccess)
        {
            return result;
        }
        
        result.Value.TryAdd("links", hyperlinks);
        
        return result;
    }
}
