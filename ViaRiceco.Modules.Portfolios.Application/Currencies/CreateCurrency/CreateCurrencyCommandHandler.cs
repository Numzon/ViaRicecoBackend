using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Application.Currencies.Models;
using ViaRiceco.Modules.Portfolios.Domain.Currencies;

namespace ViaRiceco.Modules.Portfolios.Application.Currencies.CreateCurrency;

public sealed record CreateCurrencyCommand(string Name, string Code) : ICommand<CurrencyDto>;

internal sealed class CreateCurrencyCommandHandler(
    ICurrencyRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateCurrencyCommand, CurrencyDto>
{
    public async Task<Result<CurrencyDto>> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        Result<Currency> result = Currency.Create(request.Name, request.Code, timeProvider.UtcNow());
        
        if (result.IsFailure)
        {
            return Result.Failure<CurrencyDto>(result.Error);
        }

        Currency currency = result.Value;
        repository.Insert(currency);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var currencyDto = new CurrencyDto(currency.Id, currency.Name, currency.Code);

        return currencyDto;
    }
}

[UsedImplicitly]
internal sealed class CreateCurrencyCommandValidator : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Currency name is required");

        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency code must be exactly 3 characters");
    }
}
