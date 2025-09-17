using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

namespace ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.DeleteInvestmentStrategy;

public sealed record DeleteInvestmentStrategyCommand(string Id) : ICommand;

internal sealed class DeleteInvestmentStrategyCommandHandler(
    IInvestmentStrategyRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteInvestmentStrategyCommand>
{
    public async Task<Result> Handle(DeleteInvestmentStrategyCommand request, CancellationToken cancellationToken)
    {
        InvestmentStrategy? strategy = await repository.GetAsync(request.Id, cancellationToken);

        if (strategy is null)
        {
            return Result.Failure(InvestmentStrategyErrors.NotFound(request.Id));
        }

        repository.Delete(strategy);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

[UsedImplicitly]
internal sealed class DeleteInvestmentStrategyCommandValidator : AbstractValidator<DeleteInvestmentStrategyCommand>
{
    public DeleteInvestmentStrategyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Investment strategy ID is required");
    }
}
