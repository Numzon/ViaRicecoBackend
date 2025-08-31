using System.Dynamic;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Common.Application.Services.Hyperlinks;

public interface IHyperlinkService
{
    Hyperlink Create(string endpointName, string relationship, string method, object? values = null);
}
