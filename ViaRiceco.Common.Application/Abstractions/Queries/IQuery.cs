using MediatR;
using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Common.Application.Abstractions.Queries;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
