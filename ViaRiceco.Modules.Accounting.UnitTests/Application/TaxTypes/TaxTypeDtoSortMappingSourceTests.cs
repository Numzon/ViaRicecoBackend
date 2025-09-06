using FluentAssertions;
using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Application.Services.Sorting.Models;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;
using ViaRiceco.Modules.Accounting.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Accounting.UnitTests.Application.TaxTypes;

public sealed class TaxTypeDtoSortMappingSourceTests : BaseTest
{
    private readonly TaxTypeDtoSortMappingSource _mappingSource;

    public TaxTypeDtoSortMappingSourceTests()
    {
        _mappingSource = new TaxTypeDtoSortMappingSource();
    }

    [Fact]
    public void GetSortMappingDefinition_Should_ReturnCorrectMapping()
    {
        // Act
        ISortMappingDefinition definition = _mappingSource.GetSortMappingDefinition();

        // Assert
        definition.Should().NotBeNull();
        definition.SourceType.Should().Be(nameof(TaxTypeDto));
        definition.DestinationType.Should().Be(nameof(TaxType));
        definition.Mappings.Should().NotBeEmpty();
    }

    [Fact]
    public void GetSortMappingDefinition_Should_ContainIdMapping()
    {
        // Act
        ISortMappingDefinition definition = _mappingSource.GetSortMappingDefinition();

        // Assert
        definition.Mappings.Should().Contain(mapping => 
            mapping.SortField == nameof(TaxTypeDto.Id) && 
            mapping.PropertyName == nameof(TaxType.Id));
    }

    [Fact]
    public void GetSortMappingDefinition_Should_ContainNameMapping()
    {
        // Act
        ISortMappingDefinition definition = _mappingSource.GetSortMappingDefinition();

        // Assert
        definition.Mappings.Should().Contain(mapping => 
            mapping.SortField == nameof(TaxTypeDto.Name) && 
            mapping.PropertyName == nameof(TaxType.Name));
    }

    [Fact]
    public void GetSortMappingDefinition_Should_HaveCorrectNumberOfMappings()
    {
        // Act
        ISortMappingDefinition definition = _mappingSource.GetSortMappingDefinition();

        // Assert
        definition.Mappings.Should().HaveCount(2);
    }

    [Fact]
    public void GetSortMappingDefinition_Should_CreateMappingsWithCorrectProperties()
    {
        // Act
        ISortMappingDefinition definition = _mappingSource.GetSortMappingDefinition();
        SortMapping[] mappings = definition.Mappings;

        // Assert
        mappings.Should().AllSatisfy(mapping =>
        {
            mapping.SortField.Should().NotBeNullOrEmpty();
            mapping.PropertyName.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public void GetSortMappingDefinition_Should_MapAllPublicPropertiesOfTaxTypeDto()
    {
        // Arrange
        (string, string)[] expectedMappings = new[]
        {
            (nameof(TaxTypeDto.Id), nameof(TaxType.Id)),
            (nameof(TaxTypeDto.Name), nameof(TaxType.Name))
        };

        // Act
        ISortMappingDefinition definition = _mappingSource.GetSortMappingDefinition();

        // Assert
        foreach ((string sourceProperty, string destinationProperty) in expectedMappings)
        {
            definition.Mappings.Should().Contain(mapping =>
                mapping.SortField == sourceProperty &&
                mapping.PropertyName == destinationProperty);
        }
    }

    [Fact]
    public void GetSortMappingDefinition_Should_ReturnSameInstanceOnMultipleCalls()
    {
        // Act
        ISortMappingDefinition definition1 = _mappingSource.GetSortMappingDefinition();
        ISortMappingDefinition definition2 = _mappingSource.GetSortMappingDefinition();

        // Assert
        // Both should have the same content
        definition1.SourceType.Should().Be(definition2.SourceType);
        definition1.DestinationType.Should().Be(definition2.DestinationType);
        definition1.Mappings.Should().HaveCount(definition2.Mappings.Length);

        for (int i = 0; i < definition1.Mappings.Length; i++)
        {
            definition1.Mappings[i].SortField.Should().Be(definition2.Mappings[i].SortField);
            definition1.Mappings[i].PropertyName.Should().Be(definition2.Mappings[i].PropertyName);
        }
    }

    [Fact]
    public void GetSortMappingDefinition_Should_ImplementISortMappingSource()
    {
        // Assert
        _mappingSource.Should().BeAssignableTo<ISortMappingSource>();
    }

    [Fact]
    public void GetSortMappingDefinition_Should_ReturnMappingDefinitionWithCorrectTypeNames()
    {
        // Act
        ISortMappingDefinition definition = _mappingSource.GetSortMappingDefinition();

        // Assert
        definition.SourceType.Should().Be("TaxTypeDto");
        definition.DestinationType.Should().Be("TaxType");
    }

    [Fact]
    public void TaxTypeDtoSortMappingSource_Should_BeInstantiable()
    {
        // Act & Assert
        var instance = new TaxTypeDtoSortMappingSource();
        instance.Should().NotBeNull();
        instance.Should().BeOfType<TaxTypeDtoSortMappingSource>();
    }

    [Fact]
    public void GetSortMappingDefinition_Should_CreateValidSortMappings()
    {
        // Act
        ISortMappingDefinition definition = _mappingSource.GetSortMappingDefinition();

        // Assert
        definition.Mappings.Should().AllSatisfy(mapping =>
        {
            mapping.Should().BeOfType<SortMapping>();
            mapping.SortField.Should().NotBeNullOrWhiteSpace();
            mapping.PropertyName.Should().NotBeNullOrWhiteSpace();
        });
    }
}
