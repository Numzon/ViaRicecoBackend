using MediatR;
using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Common.Application.Abstractions.Queries;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
