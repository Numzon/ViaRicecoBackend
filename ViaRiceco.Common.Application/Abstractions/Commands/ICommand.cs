using MediatR;
using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Common.Application.Abstractions;

public interface ICommand : IRequest<Result>, IBaseCommand;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

public interface IBaseCommand;
