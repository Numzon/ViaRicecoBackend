using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.InvestmentTypes.Models;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentTypes;

namespace ViaRiceco.Modules.Portfolios.Application.InvestmentTypes.GetInvestmentStrategyType;

public sealed record GetInvestmentStrategyTypeQuery(string Id) : IQuery<InvestmentStrategyTypeDto>;

internal sealed class GetInvestmentStrategyTypeQueryHandler(IInvestmentStrategyTypeRepository repository)
    : IQueryHandler<GetInvestmentStrategyTypeQuery, InvestmentStrategyTypeDto>
{
    public async Task<Result<InvestmentStrategyTypeDto>> Handle(GetInvestmentStrategyTypeQuery request, CancellationToken cancellationToken)
    {
        InvestmentStrategyType? investmentStrategyType = await repository.GetAsync(request.Id, cancellationToken);

        if (investmentStrategyType is null)
        {
            return Result.Failure<InvestmentStrategyTypeDto>(InvestmentStrategyTypeErrors.NotFound(request.Id));
        }

        var dto = new InvestmentStrategyTypeDto(investmentStrategyType.Id, investmentStrategyType.Name);

        return dto;
    }
}

[UsedImplicitly]
internal sealed class GetInvestmentStrategyTypeQueryValidator : AbstractValidator<GetInvestmentStrategyTypeQuery>
{
    public GetInvestmentStrategyTypeQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Investment strategy type ID is required");
    }
}
