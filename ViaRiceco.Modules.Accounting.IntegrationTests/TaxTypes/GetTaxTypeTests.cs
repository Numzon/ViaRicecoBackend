using System.Dynamic;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.GetTaxType;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;
using ViaRiceco.Modules.Accounting.IntegrationTests.Abstractions;
using FluentAssertions;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;

namespace ViaRiceco.Modules.Accounting.IntegrationTests.TaxTypes;

public sealed class GetTaxTypeTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnFailure_WhenCategoryDoesNotExist()
    {
        // Arrange
        string id = Guid.NewGuid().ToString();
        var query = new GetTaxTypeQuery(id);

        // Act
        Result result = await Sender.Send(query);

        // Assert
        result.Error.Should().Be(TaxTypeErrors.NotFound(query.Id));
    }

    [Fact]
    public async Task Should_ReturnCategory_WhenCategoryExists()
    {
        // Arrange
        string taxTypeId = await Sender.CreateTaxTypeAsync(Faker.Music.Genre());

        var query = new GetTaxTypeQuery(taxTypeId);

        // Act
        Result<TaxTypeDto> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}
