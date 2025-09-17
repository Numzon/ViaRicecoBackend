using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategyTypes.Models;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategyTypes;

namespace ViaRiceco.Modules.Portfolios.Application.InvestmentStrategyTypes.CreateInvestmentStrategyType;

public sealed record CreateInvestmentStrategyTypeCommand(string Name) : ICommand<InvestmentStrategyTypeDto>;

internal sealed class CreateInvestmentStrategyTypeCommandHandler(
    IInvestmentStrategyTypeRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateInvestmentStrategyTypeCommand, InvestmentStrategyTypeDto>
{
    public async Task<Result<InvestmentStrategyTypeDto>> Handle(CreateInvestmentStrategyTypeCommand request, CancellationToken cancellationToken)
    {
        Result<InvestmentStrategyType> result = InvestmentStrategyType.Create(request.Name, timeProvider.UtcNow());
        
        if (result.IsFailure)
        {
            return Result.Failure<InvestmentStrategyTypeDto>(result.Error);
        }

        InvestmentStrategyType investmentStrategyType = result.Value;
        repository.Insert(investmentStrategyType);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new InvestmentStrategyTypeDto(investmentStrategyType.Id, investmentStrategyType.Name);

        return dto;
    }
}

[UsedImplicitly]
internal sealed class CreateInvestmentStrategyTypeCommandValidator : AbstractValidator<CreateInvestmentStrategyTypeCommand>
{
    public CreateInvestmentStrategyTypeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Investment strategy type name is required");
    }
}
