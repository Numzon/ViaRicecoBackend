using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Application.Currencies.Models;
using ViaRiceco.Modules.Portfolios.Domain.Currencies;

namespace ViaRiceco.Modules.Portfolios.Application.Currencies.UpdateCurrency;

public sealed record UpdateCurrencyCommand(string Id, string Name, string Code) : ICommand<CurrencyDto>;

internal sealed class UpdateCurrencyCommandHandler(
    ICurrencyRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateCurrencyCommand, CurrencyDto>
{
    public async Task<Result<CurrencyDto>> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
    {
        Currency? currency = await repository.GetAsync(request.Id, cancellationToken);

        if (currency is null)
        {
            return Result.Failure<CurrencyDto>(CurrencyErrors.NotFound(request.Id));
        }

        Result result = currency.Update(request.Name, request.Code, timeProvider.UtcNow());

        if (result.IsFailure)
        {
            return Result.Failure<CurrencyDto>(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var currencyDto = new CurrencyDto(currency.Id, currency.Name, currency.Code);

        return currencyDto;
    }
}

[UsedImplicitly]
internal sealed class UpdateCurrencyCommandValidator : AbstractValidator<UpdateCurrencyCommand>
{
    public UpdateCurrencyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Currency ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Currency name is required");

        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency code must be exactly 3 characters");
    }
}
