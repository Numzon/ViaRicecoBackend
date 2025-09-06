using FluentAssertions;
using NSubstitute;
using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.GetTaxTypes;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;
using ViaRiceco.Modules.Accounting.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Accounting.UnitTests.Application.TaxTypes;

public sealed class GetTaxTypesQueryHandlerTests : BaseTest
{
    private readonly ITaxTypeRepository _repository;
    private readonly ISortingService _sortingService;
    private readonly GetTaxTypesQueryHandler _handler;

    public GetTaxTypesQueryHandlerTests()
    {
        _repository = Substitute.For<ITaxTypeRepository>();
        _sortingService = Substitute.For<ISortingService>();
        _handler = new GetTaxTypesQueryHandler(_repository, _sortingService);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WithValidSortParameters()
    {
        // Arrange
        string search = "Income";
        string sort = "name";
        int page = 1;
        int pageSize = 10;
        string orderBy = "Name ASC";
        int totalCount = 5;

        var taxTypes = new List<TaxType>
        {
            TaxType.Create("Income Tax", Faker.Date.RecentOffset().UtcDateTime),
            TaxType.Create("VAT", Faker.Date.RecentOffset().UtcDateTime)
        };

        var query = new GetTaxTypesQuery(search, sort, page, pageSize);

        _sortingService.ValidateSortParameters<TaxTypeDto, TaxType>(sort).Returns(true);
        _sortingService.GenerateOrderByClause<TaxTypeDto, TaxType>(sort).Returns(orderBy);
        _repository.GetPageAsync(search, orderBy, page, pageSize, Arg.Any<CancellationToken>()).Returns(taxTypes);
        _repository.CountAsync(Arg.Any<CancellationToken>()).Returns(totalCount);

        // Act
        Result<GetTaxTypesQueryResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(totalCount);
        result.Value.Items.First().Name.Should().Be("Income Tax");
        result.Value.Items.Last().Name.Should().Be("VAT");

        _sortingService.Received(1).ValidateSortParameters<TaxTypeDto, TaxType>(sort);
        _sortingService.Received(1).GenerateOrderByClause<TaxTypeDto, TaxType>(sort);
        await _repository.Received(1).GetPageAsync(search, orderBy, page, pageSize, Arg.Any<CancellationToken>());
        await _repository.Received(1).CountAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WithInvalidSortParameters()
    {
        // Arrange
        string search = "Income";
        string sort = "invalid_field";
        int page = 1;
        int pageSize = 10;

        var query = new GetTaxTypesQuery(search, sort, page, pageSize);

        _sortingService.ValidateSortParameters<TaxTypeDto, TaxType>(sort).Returns(false);

        // Act
        Result<GetTaxTypesQueryResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaxTypeErrors.InvalidSortParameter(sort));

        _sortingService.Received(1).ValidateSortParameters<TaxTypeDto, TaxType>(sort);
        _sortingService.DidNotReceive().GenerateOrderByClause<TaxTypeDto, TaxType>(Arg.Any<string>());
        await _repository.DidNotReceive().GetPageAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_HandleNullSearchParameter()
    {
        // Arrange
        string? search = null;
        string sort = "name";
        int page = 1;
        int pageSize = 10;
        string orderBy = "Name ASC";

        var taxTypes = new List<TaxType>
        {
            TaxType.Create("Income Tax", Faker.Date.RecentOffset().UtcDateTime)
        };

        var query = new GetTaxTypesQuery(search, sort, page, pageSize);

        _sortingService.ValidateSortParameters<TaxTypeDto, TaxType>(sort).Returns(true);
        _sortingService.GenerateOrderByClause<TaxTypeDto, TaxType>(sort).Returns(orderBy);
        _repository.GetPageAsync(search, orderBy, page, pageSize, Arg.Any<CancellationToken>()).Returns(taxTypes);
        _repository.CountAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        Result<GetTaxTypesQueryResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).GetPageAsync(null, orderBy, page, pageSize, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_HandleNullSortParameter()
    {
        // Arrange
        string search = "Income";
        string? sort = null;
        int page = 1;
        int pageSize = 10;
        string orderBy = "Id ASC"; // Default sort

        var taxTypes = new List<TaxType>
        {
            TaxType.Create("Income Tax", Faker.Date.RecentOffset().UtcDateTime)
        };

        var query = new GetTaxTypesQuery(search, sort, page, pageSize);

        _sortingService.ValidateSortParameters<TaxTypeDto, TaxType>(sort).Returns(true);
        _sortingService.GenerateOrderByClause<TaxTypeDto, TaxType>(sort).Returns(orderBy);
        _repository.GetPageAsync(search, orderBy, page, pageSize, Arg.Any<CancellationToken>()).Returns(taxTypes);
        _repository.CountAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        Result<GetTaxTypesQueryResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _sortingService.Received(1).ValidateSortParameters<TaxTypeDto, TaxType>(null);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyCollection_WhenNoTaxTypesFound()
    {
        // Arrange
        string search = "NonExistent";
        string sort = "name";
        int page = 1;
        int pageSize = 10;
        string orderBy = "Name ASC";
        int totalCount = 0;

        var taxTypes = new List<TaxType>();
        var query = new GetTaxTypesQuery(search, sort, page, pageSize);

        _sortingService.ValidateSortParameters<TaxTypeDto, TaxType>(sort).Returns(true);
        _sortingService.GenerateOrderByClause<TaxTypeDto, TaxType>(sort).Returns(orderBy);
        _repository.GetPageAsync(search, orderBy, page, pageSize, Arg.Any<CancellationToken>()).Returns(taxTypes);
        _repository.CountAsync(Arg.Any<CancellationToken>()).Returns(totalCount);

        // Act
        Result<GetTaxTypesQueryResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_HandleCancellationToken()
    {
        // Arrange
        string search = "Income";
        string sort = "name";
        int page = 1;
        int pageSize = 10;
        string orderBy = "Name ASC";
        var cancellationToken = new CancellationToken(true);

        var taxTypes = new List<TaxType>
        {
            TaxType.Create("Income Tax", Faker.Date.RecentOffset().UtcDateTime)
        };

        var query = new GetTaxTypesQuery(search, sort, page, pageSize);

        _sortingService.ValidateSortParameters<TaxTypeDto, TaxType>(sort).Returns(true);
        _sortingService.GenerateOrderByClause<TaxTypeDto, TaxType>(sort).Returns(orderBy);
        _repository.GetPageAsync(search, orderBy, page, pageSize, cancellationToken).Returns(taxTypes);
        _repository.CountAsync(cancellationToken).Returns(1);

        // Act
        Result<GetTaxTypesQueryResponse> result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).GetPageAsync(search, orderBy, page, pageSize, cancellationToken);
        await _repository.Received(1).CountAsync(cancellationToken);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(2, 5)]
    [InlineData(3, 20)]
    public async Task Handle_Should_HandleDifferentPaginationParameters(int page, int pageSize)
    {
        // Arrange
        string search = "Tax";
        string sort = "name";
        string orderBy = "Name ASC";

        var taxTypes = new List<TaxType>
        {
            TaxType.Create("Income Tax", Faker.Date.RecentOffset().UtcDateTime)
        };

        var query = new GetTaxTypesQuery(search, sort, page, pageSize);

        _sortingService.ValidateSortParameters<TaxTypeDto, TaxType>(sort).Returns(true);
        _sortingService.GenerateOrderByClause<TaxTypeDto, TaxType>(sort).Returns(orderBy);
        _repository.GetPageAsync(search, orderBy, page, pageSize, Arg.Any<CancellationToken>()).Returns(taxTypes);
        _repository.CountAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        Result<GetTaxTypesQueryResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).GetPageAsync(search, orderBy, page, pageSize, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_MapTaxTypesToDtos()
    {
        // Arrange
        string search = "Tax";
        string sort = "name";
        string orderBy = "Name ASC";

        var taxType1 = TaxType.Create("Income Tax", Faker.Date.RecentOffset().UtcDateTime);
        var taxType2 = TaxType.Create("VAT", Faker.Date.RecentOffset().UtcDateTime);
        var taxTypes = new List<TaxType> { taxType1, taxType2 };

        var query = new GetTaxTypesQuery(search, sort, 1, 10);

        _sortingService.ValidateSortParameters<TaxTypeDto, TaxType>(sort).Returns(true);
        _sortingService.GenerateOrderByClause<TaxTypeDto, TaxType>(sort).Returns(orderBy);
        _repository.GetPageAsync(search, orderBy, 1, 10, Arg.Any<CancellationToken>()).Returns(taxTypes);
        _repository.CountAsync(Arg.Any<CancellationToken>()).Returns(2);

        // Act
        Result<GetTaxTypesQueryResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        
        TaxTypeDto firstDto = result.Value.Items.First();
        firstDto.Id.Should().Be(taxType1.Id);
        firstDto.Name.Should().Be("Income Tax");

        TaxTypeDto lastDto = result.Value.Items.Last();
        lastDto.Id.Should().Be(taxType2.Id);
        lastDto.Name.Should().Be("VAT");
    }

    [Fact]
    public async Task Handle_Should_ReturnCorrectTotalCount()
    {
        // Arrange
        string search = "Tax";
        string sort = "name";
        string orderBy = "Name ASC";
        int expectedTotalCount = 100;

        var taxTypes = new List<TaxType>
        {
            TaxType.Create("Income Tax", Faker.Date.RecentOffset().UtcDateTime)
        };

        var query = new GetTaxTypesQuery(search, sort, 1, 10);

        _sortingService.ValidateSortParameters<TaxTypeDto, TaxType>(sort).Returns(true);
        _sortingService.GenerateOrderByClause<TaxTypeDto, TaxType>(sort).Returns(orderBy);
        _repository.GetPageAsync(search, orderBy, 1, 10, Arg.Any<CancellationToken>()).Returns(taxTypes);
        _repository.CountAsync(Arg.Any<CancellationToken>()).Returns(expectedTotalCount);

        // Act
        Result<GetTaxTypesQueryResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(expectedTotalCount);
    }
}
