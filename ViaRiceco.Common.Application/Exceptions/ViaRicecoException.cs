using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Common.Application.Exceptions;

public class ViaRicecoException(string requestName, Error? error = null, Exception? innerException = null)
    : Exception("Application exception", innerException)
{
    public string RequestName { get; } = requestName;
    public Error? Error { get; } = error;
}
