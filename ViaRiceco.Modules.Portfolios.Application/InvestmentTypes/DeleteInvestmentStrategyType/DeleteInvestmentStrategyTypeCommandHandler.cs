using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentTypes;

namespace ViaRiceco.Modules.Portfolios.Application.InvestmentTypes.DeleteInvestmentStrategyType;

public sealed record DeleteInvestmentStrategyTypeCommand(string Id) : ICommand;

internal sealed class DeleteInvestmentStrategyTypeCommandHandler(
    IInvestmentStrategyTypeRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteInvestmentStrategyTypeCommand>
{
    public async Task<Result> Handle(DeleteInvestmentStrategyTypeCommand request, CancellationToken cancellationToken)
    {
        InvestmentStrategyType? investmentStrategyType = await repository.GetAsync(request.Id, cancellationToken);

        if (investmentStrategyType is null)
        {
            return Result.Failure(InvestmentStrategyTypeErrors.NotFound(request.Id));
        }

        repository.Delete(investmentStrategyType);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

[UsedImplicitly]
internal sealed class DeleteInvestmentStrategyTypeCommandValidator : AbstractValidator<DeleteInvestmentStrategyTypeCommand>
{
    public DeleteInvestmentStrategyTypeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Investment strategy type ID is required");
    }
}
