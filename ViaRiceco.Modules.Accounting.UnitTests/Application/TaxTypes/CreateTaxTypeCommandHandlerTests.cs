using FluentAssertions;
using NSubstitute;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.CreateTaxType;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;
using ViaRiceco.Modules.Accounting.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Accounting.UnitTests.Application.TaxTypes;

public sealed class CreateTaxTypeCommandHandlerTests : BaseTest
{
    private readonly ITaxTypeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly CreateTaxTypeCommandHandler _handler;

    public CreateTaxTypeCommandHandlerTests()
    {
        _repository = Substitute.For<ITaxTypeRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeProvider = Substitute.For<TimeProvider>();
        _handler = new CreateTaxTypeCommandHandler(_repository, _unitOfWork, _timeProvider);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenTaxTypeNameIsUnique()
    {
        // Arrange
        string name = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        var command = new CreateTaxTypeCommand(name);

        _repository.ExistsByNameAsync(name, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(createdAtUtc);

        // Act
        Result<TaxTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Id.Should().StartWith("tt_");

        _repository.Received(1).Insert(Arg.Is<TaxType>(t => t.Name == name));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenTaxTypeNameAlreadyExists()
    {
        // Arrange
        string name = Faker.Commerce.Categories(1)[0];
        var command = new CreateTaxTypeCommand(name);

        _repository.ExistsByNameAsync(name, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        Result<TaxTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaxTypeErrors.NameNotUnique(name));

        _repository.DidNotReceive().Insert(Arg.Any<TaxType>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Income Tax")]
    [InlineData("VAT")]
    [InlineData("Corporate Tax")]
    [InlineData("Property Tax")]
    public async Task Handle_Should_CreateTaxTypeWithCorrectName(string name)
    {
        // Arrange
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        var command = new CreateTaxTypeCommand(name);

        _repository.ExistsByNameAsync(name, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(createdAtUtc);

        // Act
        Result<TaxTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Id.Should().StartWith("tt_");

        _repository.Received(1).Insert(Arg.Is<TaxType>(t => 
            t.Name == name && 
            t.CreatedAtUtc == createdAtUtc &&
            t.Id.StartsWith("tt_")));
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryWithCorrectParameters()
    {
        // Arrange
        string name = Faker.Commerce.Categories(1)[0];
        var command = new CreateTaxTypeCommand(name);

        _repository.ExistsByNameAsync(name, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).ExistsByNameAsync(name, Arg.Any<CancellationToken>());
        _repository.Received(1).Insert(Arg.Is<TaxType>(t => t.Name == name));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseTimeProviderForCreatedAt()
    {
        // Arrange
        string name = Faker.Commerce.Categories(1)[0];
        DateTime expectedCreatedAt = Faker.Date.RecentOffset().UtcDateTime;
        var command = new CreateTaxTypeCommand(name);

        _repository.ExistsByNameAsync(name, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(expectedCreatedAt);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Received(1).Insert(Arg.Is<TaxType>(t => t.CreatedAtUtc == expectedCreatedAt));
    }

    [Fact]
    public async Task Handle_Should_HandleCancellationToken()
    {
        // Arrange
        string name = Faker.Commerce.Categories(1)[0];
        var command = new CreateTaxTypeCommand(name);
        var cancellationToken = new CancellationToken(true);

        _repository.ExistsByNameAsync(name, cancellationToken).Returns(false);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        await _repository.Received(1).ExistsByNameAsync(name, cancellationToken);
        await _unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
    }
}
