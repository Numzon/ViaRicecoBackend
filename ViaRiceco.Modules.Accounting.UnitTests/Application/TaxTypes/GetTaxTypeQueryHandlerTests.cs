using FluentAssertions;
using NSubstitute;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.GetTaxType;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;
using ViaRiceco.Modules.Accounting.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Accounting.UnitTests.Application.TaxTypes;

public sealed class GetTaxTypeQueryHandlerTests : BaseTest
{
    private readonly ITaxTypeRepository _repository;
    private readonly GetTaxTypeQueryHandler _handler;

    public GetTaxTypeQueryHandlerTests()
    {
        _repository = Substitute.For<ITaxTypeRepository>();
        _handler = new GetTaxTypeQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenTaxTypeExists()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string name = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var taxType = TaxType.Create(name, createdAtUtc);
        var query = new GetTaxTypeQuery(taxTypeId);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns(taxType);

        // Act
        Result<TaxTypeDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(taxType.Id);
        result.Value.Name.Should().Be(name);

        await _repository.Received(1).GetAsync(taxTypeId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenTaxTypeDoesNotExist()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        var query = new GetTaxTypeQuery(taxTypeId);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns((TaxType?)null);

        // Act
        Result<TaxTypeDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaxTypeErrors.NotFound(taxTypeId));

        await _repository.Received(1).GetAsync(taxTypeId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryWithCorrectId()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string name = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var taxType = TaxType.Create(name, createdAtUtc);
        var query = new GetTaxTypeQuery(taxTypeId);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns(taxType);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _repository.Received(1).GetAsync(taxTypeId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_HandleCancellationToken()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string name = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var taxType = TaxType.Create(name, createdAtUtc);
        var query = new GetTaxTypeQuery(taxTypeId);
        var cancellationToken = new CancellationToken(true);

        _repository.GetAsync(taxTypeId, cancellationToken).Returns(taxType);

        // Act
        Result<TaxTypeDto> result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).GetAsync(taxTypeId, cancellationToken);
    }

    [Theory]
    [InlineData("Income Tax")]
    [InlineData("VAT")]
    [InlineData("Corporate Tax")]
    [InlineData("Property Tax")]
    public async Task Handle_Should_ReturnCorrectTaxTypeDto(string name)
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var taxType = TaxType.Create(name, createdAtUtc);
        var query = new GetTaxTypeQuery(taxTypeId);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns(taxType);

        // Act
        Result<TaxTypeDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<TaxTypeDto>();
        result.Value.Id.Should().Be(taxType.Id);
        result.Value.Name.Should().Be(name);
    }

    [Fact]
    public async Task Handle_Should_ReturnTaxTypeDtoWithCorrectValues()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string name = "Test Tax Type";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var taxType = TaxType.Create(name, createdAtUtc);
        var query = new GetTaxTypeQuery(taxTypeId);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns(taxType);

        // Act
        Result<TaxTypeDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().StartWith("tt_");
        result.Value.Name.Should().Be(name);
        result.Value.Id.Should().Be(taxType.Id);
    }

    [Theory]
    [InlineData("tt_12345")]
    [InlineData("tt_abcdef")]
    public async Task Handle_Should_HandleDifferentTaxTypeIds(string taxTypeId)
    {
        // Arrange
        string name = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var taxType = TaxType.Create(name, createdAtUtc);
        var query = new GetTaxTypeQuery(taxTypeId);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns(taxType);

        // Act
        Result<TaxTypeDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).GetAsync(taxTypeId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotCallRepositoryMultipleTimes()
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string name = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var taxType = TaxType.Create(name, createdAtUtc);
        var query = new GetTaxTypeQuery(taxTypeId);

        _repository.GetAsync(taxTypeId, Arg.Any<CancellationToken>()).Returns(taxType);

        // Act
        Result<TaxTypeDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
