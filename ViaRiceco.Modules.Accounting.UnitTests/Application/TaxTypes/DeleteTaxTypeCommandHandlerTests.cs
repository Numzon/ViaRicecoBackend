using FluentAssertions;
using NSubstitute;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.Abstractions.Data;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.DeleteTaxType;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;
using ViaRiceco.Modules.Accounting.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Accounting.UnitTests.Application.TaxTypes;

public sealed class DeleteTaxTypeCommandHandlerTests : BaseTest
{
    private readonly ITaxTypeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteTaxTypeCommandHandler _handler;

    public DeleteTaxTypeCommandHandlerTests()
    {
        _repository = Substitute.For<ITaxTypeRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new DeleteTaxTypeCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenTaxTypeExists()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string name = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        
        var taxType = TaxType.Create(name, createdAtUtc);
        var command = new DeleteTaxTypeCommand(taxTypeId);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns(taxType);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repository.Received(1).Delete(taxType);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenTaxTypeDoesNotExist()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        var command = new DeleteTaxTypeCommand(taxTypeId);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns((TaxType?)null);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaxTypeErrors.NotFound(taxTypeId));

        _repository.DidNotReceive().Delete(Arg.Any<TaxType>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryWithCorrectId()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string name = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        
        var taxType = TaxType.Create(name, createdAtUtc);
        var command = new DeleteTaxTypeCommand(taxTypeId);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns(taxType);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).GetAsync(taxTypeId, Arg.Any<CancellationToken>());
        _repository.Received(1).Delete(taxType);
    }

    [Fact]
    public async Task Handle_Should_HandleCancellationToken()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string name = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        
        var taxType = TaxType.Create(name, createdAtUtc);
        var command = new DeleteTaxTypeCommand(taxTypeId);
        var cancellationToken = new CancellationToken(true);

        _repository.GetAsync(taxTypeId, cancellationToken).Returns(taxType);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        await _repository.Received(1).GetAsync(taxTypeId, cancellationToken);
        await _unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
    }

    [Theory]
    [InlineData("tt_12345")]
    [InlineData("tt_abcdef")]
    public async Task Handle_Should_DeleteTaxTypeWithSpecificId(string taxTypeId)
    {
        // Arrange
        string name = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        
        var taxType = TaxType.Create(name, createdAtUtc);
        var command = new DeleteTaxTypeCommand(taxTypeId);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns(taxType);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).GetAsync(taxTypeId, Arg.Any<CancellationToken>());
        _repository.Received(1).Delete(taxType);
    }

    [Fact]
    public async Task Handle_Should_NotCallSaveChanges_WhenTaxTypeNotFound()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        var command = new DeleteTaxTypeCommand(taxTypeId);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns((TaxType?)null);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_OnlyCallDeleteOnce_WhenTaxTypeExists()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string name = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        
        var taxType = TaxType.Create(name, createdAtUtc);
        var command = new DeleteTaxTypeCommand(taxTypeId);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns(taxType);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Received(1).Delete(taxType);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
