using FluentAssertions;
using ViaRiceco.Modules.Budgets.Application.MonthlyBudgetExpenses.BulkSetExpenseValues;
using ViaRiceco.Modules.Budgets.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.UnitTests.Application.MonthlyBudgets;

public sealed class BulkSetExpenseValuesCommandValidatorTests : BaseTest
{
    private readonly BulkSetExpenseValuesCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_ReturnSuccess_WhenCommandIsValid()
    {
        // Arrange
        string monthlyBudgetId = "mb_" + Faker.Random.Guid();
        var expenseUpdates = new List<ExpenseValueUpdateDto>
        {
            new("mbe_" + Faker.Random.Guid(), 100.50m),
            new("mbe_" + Faker.Random.Guid(), 200.75m)
        };
        var command = new BulkSetExpenseValuesCommand(monthlyBudgetId, expenseUpdates);

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_ReturnFailure_WhenMonthlyBudgetIdIsEmpty()
    {
        // Arrange
        var expenseUpdates = new List<ExpenseValueUpdateDto>
        {
            new("mbe_" + Faker.Random.Guid(), 100.50m)
        };
        var command = new BulkSetExpenseValuesCommand(string.Empty, expenseUpdates);

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Monthly budget ID is required");
    }

    [Fact]
    public void Validate_Should_ReturnFailure_WhenExpenseValueUpdatesIsEmpty()
    {
        // Arrange
        string monthlyBudgetId = "mb_" + Faker.Random.Guid();
        var expenseUpdates = new List<ExpenseValueUpdateDto>();
        var command = new BulkSetExpenseValuesCommand(monthlyBudgetId, expenseUpdates);

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "At least one expense value update is required");
    }

    [Fact]
    public void Validate_Should_ReturnFailure_WhenTooManyExpenseUpdates()
    {
        // Arrange
        string monthlyBudgetId = "mb_" + Faker.Random.Guid();
        var expenseUpdates = new List<ExpenseValueUpdateDto>();
        
        // Create 101 updates (exceeds the limit of 100)
        for (int i = 0; i < 101; i++)
        {
            expenseUpdates.Add(new ExpenseValueUpdateDto("mbe_" + Faker.Random.Guid(), 100m));
        }
        
        var command = new BulkSetExpenseValuesCommand(monthlyBudgetId, expenseUpdates);

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Cannot update more than 100 expenses at once");
    }

    [Fact]
    public void Validate_Should_ReturnFailure_WhenMonthlyBudgetExpenseIdIsEmpty()
    {
        // Arrange
        string monthlyBudgetId = "mb_" + Faker.Random.Guid();
        var expenseUpdates = new List<ExpenseValueUpdateDto>
        {
            new(string.Empty, 100.50m)
        };
        var command = new BulkSetExpenseValuesCommand(monthlyBudgetId, expenseUpdates);

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Monthly budget expense ID is required");
    }

    [Fact]
    public void Validate_Should_ReturnFailure_WhenExpenseValueIsNegative()
    {
        // Arrange
        string monthlyBudgetId = "mb_" + Faker.Random.Guid();
        var expenseUpdates = new List<ExpenseValueUpdateDto>
        {
            new("mbe_" + Faker.Random.Guid(), -50.25m)
        };
        var command = new BulkSetExpenseValuesCommand(monthlyBudgetId, expenseUpdates);

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Expense value must be greater than or equal to 0");
    }

    [Fact]
    public void Validate_Should_ReturnSuccess_WhenExpenseValueIsNull()
    {
        // Arrange
        string monthlyBudgetId = "mb_" + Faker.Random.Guid();
        var expenseUpdates = new List<ExpenseValueUpdateDto>
        {
            new("mbe_" + Faker.Random.Guid(), null)
        };
        var command = new BulkSetExpenseValuesCommand(monthlyBudgetId, expenseUpdates);

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_ReturnSuccess_WhenExpenseValueIsZero()
    {
        // Arrange
        string monthlyBudgetId = "mb_" + Faker.Random.Guid();
        var expenseUpdates = new List<ExpenseValueUpdateDto>
        {
            new("mbe_" + Faker.Random.Guid(), 0m)
        };
        var command = new BulkSetExpenseValuesCommand(monthlyBudgetId, expenseUpdates);

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_ReturnFailure_WhenMultipleExpensesHaveErrors()
    {
        // Arrange
        string monthlyBudgetId = "mb_" + Faker.Random.Guid();
        var expenseUpdates = new List<ExpenseValueUpdateDto>
        {
            new(string.Empty, -100m), // Both ID empty and value negative
            new("mbe_" + Faker.Random.Guid(), -50m) // Value negative
        };
        var command = new BulkSetExpenseValuesCommand(monthlyBudgetId, expenseUpdates);

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(c => c >= 3); // At least 3 errors
        result.Errors.Should().Contain(e => e.ErrorMessage == "Monthly budget expense ID is required");
        result.Errors.Should().Contain(e => e.ErrorMessage == "Expense value must be greater than or equal to 0");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_Should_ReturnSuccess_WhenExpenseCountIsWithinLimit(int expenseCount)
    {
        // Arrange
        string monthlyBudgetId = "mb_" + Faker.Random.Guid();
        var expenseUpdates = new List<ExpenseValueUpdateDto>();
        
        for (int i = 0; i < expenseCount; i++)
        {
            expenseUpdates.Add(new ExpenseValueUpdateDto("mbe_" + Faker.Random.Guid(), 100m));
        }
        
        var command = new BulkSetExpenseValuesCommand(monthlyBudgetId, expenseUpdates);

        // Act
        FluentValidation.Results.ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
