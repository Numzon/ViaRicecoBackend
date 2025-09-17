using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Currencies.Models;
using ViaRiceco.Modules.Portfolios.Domain.Currencies;

namespace ViaRiceco.Modules.Portfolios.Application.Currencies.GetCurrency;

public sealed record GetCurrencyQuery(string Id) : IQuery<CurrencyDto>;

internal sealed class GetCurrencyQueryHandler(ICurrencyRepository repository)
    : IQueryHandler<GetCurrencyQuery, CurrencyDto>
{
    public async Task<Result<CurrencyDto>> Handle(GetCurrencyQuery request, CancellationToken cancellationToken)
    {
        Currency? currency = await repository.GetAsync(request.Id, cancellationToken);

        if (currency is null)
        {
            return Result.Failure<CurrencyDto>(CurrencyErrors.NotFound(request.Id));
        }

        var currencyDto = new CurrencyDto(currency.Id, currency.Name, currency.Code);

        return currencyDto;
    }
}

[UsedImplicitly]
internal sealed class GetCurrencyQueryValidator : AbstractValidator<GetCurrencyQuery>
{
    public GetCurrencyQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Currency ID is required");
    }
}
