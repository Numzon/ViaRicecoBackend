using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.Currencies;

namespace ViaRiceco.Modules.Portfolios.Application.Currencies.DeleteCurrency;

public sealed record DeleteCurrencyCommand(string Id) : ICommand;

internal sealed class DeleteCurrencyCommandHandler(
    ICurrencyRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCurrencyCommand>
{
    public async Task<Result> Handle(DeleteCurrencyCommand request, CancellationToken cancellationToken)
    {
        Currency? currency = await repository.GetAsync(request.Id, cancellationToken);

        if (currency is null)
        {
            return Result.Failure(CurrencyErrors.NotFound(request.Id));
        }

        repository.Delete(currency);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

[UsedImplicitly]
internal sealed class DeleteCurrencyCommandValidator : AbstractValidator<DeleteCurrencyCommand>
{
    public DeleteCurrencyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Currency ID is required");
    }
}
