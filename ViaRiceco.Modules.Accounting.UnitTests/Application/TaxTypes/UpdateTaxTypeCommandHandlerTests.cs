using FluentAssertions;
using NSubstitute;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.UpdateTaxType;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;
using ViaRiceco.Modules.Accounting.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Accounting.UnitTests.Application.TaxTypes;

public sealed class UpdateTaxTypeCommandHandlerTests : BaseTest
{
    private readonly ITaxTypeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly UpdateTaxTypeCommandHandler _handler;

    public UpdateTaxTypeCommandHandlerTests()
    {
        _repository = Substitute.For<ITaxTypeRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeProvider = Substitute.For<TimeProvider>();
        _handler = new UpdateTaxTypeCommandHandler(_repository, _unitOfWork, _timeProvider);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenTaxTypeExistsAndNameIsUnique()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string originalName = "Income Tax";
        string newName = "Updated Income Tax";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var taxType = TaxType.Create(originalName, createdAtUtc);
        var command = new UpdateTaxTypeCommand(taxTypeId, newName);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns(taxType);
        _repository.ExistsByNameAsync(newName, taxTypeId, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(updatedAtUtc);

        // Act
        Result<TaxTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(newName);
        result.Value.Id.Should().Be(taxType.Id);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenTaxTypeDoesNotExist()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string newName = "Updated Tax";
        var command = new UpdateTaxTypeCommand(taxTypeId, newName);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns((TaxType?)null);

        // Act
        Result<TaxTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaxTypeErrors.NotFound(taxTypeId));

        await _repository.DidNotReceive().ExistsByNameAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenNameAlreadyExistsForDifferentTaxType()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string originalName = "Income Tax";
        string newName = "VAT Tax"; // Name that already exists for another tax type
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;

        var taxType = TaxType.Create(originalName, createdAtUtc);
        var command = new UpdateTaxTypeCommand(taxTypeId, newName);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns(taxType);
        _repository.ExistsByNameAsync(newName, taxTypeId, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        Result<TaxTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaxTypeErrors.NameNotUnique(newName));

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_CallTaxTypeUpdateWithCorrectParameters()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string originalName = "Income Tax";
        string newName = "Updated Income Tax";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var taxType = TaxType.Create(originalName, createdAtUtc);
        var command = new UpdateTaxTypeCommand(taxTypeId, newName);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns(taxType);
        _repository.ExistsByNameAsync(newName, taxTypeId, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(updatedAtUtc);

        // Act
        Result<TaxTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        taxType.Name.Should().Be(newName);
        taxType.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public async Task Handle_Should_HandleCancellationToken()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string originalName = "Income Tax";
        string newName = "Updated Income Tax";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        
        var taxType = TaxType.Create(originalName, createdAtUtc);
        var command = new UpdateTaxTypeCommand(taxTypeId, newName);
        var cancellationToken = new CancellationToken(true);

        _repository.GetAsync(taxTypeId, cancellationToken).Returns(taxType);
        _repository.ExistsByNameAsync(newName, taxTypeId, cancellationToken).Returns(false);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        await _repository.Received(1).GetAsync(taxTypeId, cancellationToken);
        await _repository.Received(1).ExistsByNameAsync(newName, taxTypeId, cancellationToken);
        await _unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_UseTimeProviderForUpdatedAt()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string originalName = "Income Tax";
        string newName = "Updated Income Tax";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime expectedUpdatedAt = Faker.Date.RecentOffset().UtcDateTime;

        var taxType = TaxType.Create(originalName, createdAtUtc);
        var command = new UpdateTaxTypeCommand(taxTypeId, newName);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns(taxType);
        _repository.ExistsByNameAsync(newName, taxTypeId, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(expectedUpdatedAt);

        // Act
        Result<TaxTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        taxType.UpdatedAtUtc.Should().Be(expectedUpdatedAt);
    }

    [Theory]
    [InlineData("Corporate Tax")]
    [InlineData("Property Tax")]
    [InlineData("Sales Tax")]
    public async Task Handle_Should_UpdateTaxTypeWithCorrectName(string newName)
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string originalName = "Income Tax";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var taxType = TaxType.Create(originalName, createdAtUtc);
        var command = new UpdateTaxTypeCommand(taxTypeId, newName);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns(taxType);
        _repository.ExistsByNameAsync(newName, taxTypeId, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(updatedAtUtc);

        // Act
        Result<TaxTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(newName);
        taxType.Name.Should().Be(newName);
    }

    [Fact]
    public async Task Handle_Should_ReturnUpdatedTaxTypeDto()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string originalName = "Income Tax";
        string newName = "Updated Income Tax";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;

        var taxType = TaxType.Create(originalName, createdAtUtc);
        var command = new UpdateTaxTypeCommand(taxTypeId, newName);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns(taxType);
        _repository.ExistsByNameAsync(newName, taxTypeId, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        Result<TaxTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<TaxTypeDto>();
        result.Value.Id.Should().Be(taxType.Id);
        result.Value.Name.Should().Be(newName);
    }

    [Fact]
    public async Task Handle_Should_AllowSameNameForSameTaxType()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string sameName = "Income Tax";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var taxType = TaxType.Create(sameName, createdAtUtc);
        var command = new UpdateTaxTypeCommand(taxTypeId, sameName);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns(taxType);
        _repository.ExistsByNameAsync(sameName, taxTypeId, Arg.Any<CancellationToken>()).Returns(false);
        _timeProvider.GetUtcNow().Returns(updatedAtUtc);

        // Act
        Result<TaxTypeDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(sameName);
    }
}
