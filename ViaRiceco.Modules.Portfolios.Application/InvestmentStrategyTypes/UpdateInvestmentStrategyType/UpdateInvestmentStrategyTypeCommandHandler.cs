using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategyTypes.Models;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategyTypes;

namespace ViaRiceco.Modules.Portfolios.Application.InvestmentStrategyTypes.UpdateInvestmentStrategyType;

public sealed record UpdateInvestmentStrategyTypeCommand(string Id, string Name) : ICommand<InvestmentStrategyTypeDto>;

internal sealed class UpdateInvestmentStrategyTypeCommandHandler(
    IInvestmentStrategyTypeRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateInvestmentStrategyTypeCommand, InvestmentStrategyTypeDto>
{
    public async Task<Result<InvestmentStrategyTypeDto>> Handle(UpdateInvestmentStrategyTypeCommand request, CancellationToken cancellationToken)
    {
        InvestmentStrategyType? investmentStrategyType = await repository.GetAsync(request.Id, cancellationToken);

        if (investmentStrategyType is null)
        {
            return Result.Failure<InvestmentStrategyTypeDto>(InvestmentStrategyTypeErrors.NotFound(request.Id));
        }

        Result result = investmentStrategyType.Update(request.Name, timeProvider.UtcNow());

        if (result.IsFailure)
        {
            return Result.Failure<InvestmentStrategyTypeDto>(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new InvestmentStrategyTypeDto(investmentStrategyType.Id, investmentStrategyType.Name);

        return dto;
    }
}

[UsedImplicitly]
internal sealed class UpdateInvestmentStrategyTypeCommandValidator : AbstractValidator<UpdateInvestmentStrategyTypeCommand>
{
    public UpdateInvestmentStrategyTypeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Investment strategy type ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Investment strategy type name is required");
    }
}
