using FluentAssertions;
using ViaRiceco.Common.Domain.Enumerations;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.IntegrationTests.Abstractions;
using ViaRiceco.Modules.Accounting.Application.Incomes.AddIncome;
using ViaRiceco.Modules.Accounting.Application.Incomes.RemoveIncome;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.CreateSettlementPeriod;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.DeleteSettlementPeriod;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.GetSettlementPeriod;
using ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models;
using ViaRiceco.Modules.Accounting.Application.Taxes.AddTax;
using ViaRiceco.Modules.Accounting.Application.Taxes.RemoveTax;
using ViaRiceco.Modules.Budgets.Application.Expenses.CreateExpense;
using ViaRiceco.Modules.Budgets.Application.Expenses.DeleteExpense;
using ViaRiceco.Modules.Budgets.Application.Expenses.GetExpense;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.CreateExpenseType;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.DeleteExpenseType;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.GetExpenseTypes;
using ViaRiceco.Modules.Portfolios.Application.FinancialGoals.CreateFinancialGoal;
using ViaRiceco.Modules.Portfolios.Application.FinancialGoals.DeleteFinancialGoal;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.CreateTaxType;
using ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;
using ViaRiceco.Modules.Budgets.Application.Expenses.Models;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Portfolios.Application.FinancialGoals.Models;

namespace ViaRiceco.IntegrationTests;

/// <summary>
/// Comprehensive edge case and error handling tests for ViaRiceco system
/// </summary>
public sealed class EdgeCaseAndErrorHandlingTests : BaseIntegrationTest
{
    public EdgeCaseAndErrorHandlingTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_HandleCascadingDeletes_AndOrphanedDataScenarios()
    {
        // Arrange - Create interconnected data
        SettlementPeriodDto settlementPeriod = await Sender.CreateSettlementPeriodAsync();
        await Sender.AddIncomeToSettlementPeriodAsync(settlementPeriod.Id, 3000);
        
        TaxTypeDto taxType = await Sender.CreateTaxTypeAsync("Deletable Tax Type");
        await Sender.AddTaxToSettlementPeriodAsync(settlementPeriod.Id, taxType.Id, 600);

        // Act - Test cascading delete scenarios
        
        // 1. Try to delete settlement period with dependencies
        var deleteSettlementCommand = new DeleteSettlementPeriodCommand(settlementPeriod.Id);
        Result deleteSettlementResult = await Sender.Send(deleteSettlementCommand);

        // Settlement period deletion might fail if it has dependencies, or succeed with cascade
        if (deleteSettlementResult.IsFailure)
        {
            // If deletion failed, the settlement should still exist with its data
            var verifyExistsQuery = new GetSettlementPeriodQuery(settlementPeriod.Id);
            Result<SettlementPeriodDto> verifyResult = await Sender.Send(verifyExistsQuery);
            verifyResult.IsSuccess.Should().BeTrue("Settlement period should still exist if delete failed");
        }
        else
        {
            // If deletion succeeded, verify cascading worked properly
            var verifyDeletedQuery = new GetSettlementPeriodQuery(settlementPeriod.Id);
            Result<SettlementPeriodDto> verifyResult = await Sender.Send(verifyDeletedQuery);
            verifyResult.IsFailure.Should().BeTrue("Settlement period should be deleted");
            verifyResult.Error.Type.Should().Be(ErrorType.NotFound);
        }
    }

    [Fact]
    public async Task Should_HandleNonExistentIds_AcrossAllModules()
    {
        // Test scenarios with non-existent IDs
        string?[] nonExistentIds = new[]
        {
            "sp_00000000-0000-0000-0000-000000000000",
            "i_00000000-0000-0000-0000-000000000000", 
            "t_00000000-0000-0000-0000-000000000000",
            "tt_00000000-0000-0000-0000-000000000000",
            "nonexistent_id",
            "",
            null
        };

        foreach (string? invalidId in nonExistentIds.Where(id => !string.IsNullOrEmpty(id)))
        {
            // Test getting non-existent settlement period
            var getSettlementQuery = new GetSettlementPeriodQuery(invalidId!);
            Result<SettlementPeriodDto> getSettlementResult = await Sender.Send(getSettlementQuery);
            getSettlementResult.IsFailure.Should().BeTrue($"Getting settlement period with ID '{invalidId}' should fail");
            
            if (!string.IsNullOrEmpty(invalidId))
            {
                getSettlementResult.Error.Type.Should().Be(ErrorType.NotFound);
            }
            else
            {
                getSettlementResult.Error.Type.Should().Be(ErrorType.Validation);
            }

            // Test adding income to non-existent settlement period
            var addIncomeCommand = new AddIncomeCommand(invalidId!, 1000);
            Result<IncomeDto> addIncomeResult = await Sender.Send(addIncomeCommand);
            addIncomeResult.IsFailure.Should().BeTrue($"Adding income to settlement '{invalidId}' should fail");
        }

        // Test null ID scenarios
        var nullIdCommand = new AddIncomeCommand(null!, 1000);
        Result<IncomeDto> nullIdResult = await Sender.Send(nullIdCommand);
        nullIdResult.IsFailure.Should().BeTrue("Null settlement period ID should be rejected");
        nullIdResult.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Should_HandleExtremelyLongStrings_AndSpecialCharacters()
    {
        // Test string length limits and special characters
        (string testName, bool shouldSucceed)[] extremeStringTestCases = new[]
        {
            ("Normal Name", true),
            (new string('A', 200), true), // At limit
            (new string('B', 201), false), // Over limit
            (new string('C', 1000), false), // Way over limit
            ("Name with émojis 🚀💰📊", true), // Unicode characters
            ("Name with\nnewlines\tand\ttabs", true), // Control characters
            ("", false), // Empty string
            ("   ", false), // Just whitespace
            ("Name with <script>alert('xss')</script>", true), // Potential XSS (should be handled by validation/encoding)
            ("Name with SQL'; DROP TABLE Users;--", true) // Potential SQL injection
        };

        foreach ((string testName, bool shouldSucceed) in extremeStringTestCases)
        {
            // Test tax type creation with extreme strings
            var taxTypeCommand = new CreateTaxTypeCommand(testName);
            Result<TaxTypeDto> taxTypeResult = await Sender.Send(taxTypeCommand);

            if (shouldSucceed)
            {
                taxTypeResult.IsSuccess.Should().BeTrue($"Tax type with name '{testName.Substring(0, Math.Min(50, testName.Length))}...' should succeed");
            }
            else
            {
                taxTypeResult.IsFailure.Should().BeTrue($"Tax type with name '{testName.Substring(0, Math.Min(50, testName.Length))}...' should fail");
                taxTypeResult.Error.Type.Should().Be(ErrorType.Validation);
            }

            // Test expense type creation
            var expenseTypeCommand = new CreateExpenseTypeCommand(testName);
            Result<ExpenseTypeDto> expenseTypeResult = await Sender.Send(expenseTypeCommand);

            if (shouldSucceed)
            {
                expenseTypeResult.IsSuccess.Should().BeTrue($"Expense type with name '{testName.Substring(0, Math.Min(50, testName.Length))}...' should succeed");
            }
            else
            {
                expenseTypeResult.IsFailure.Should().BeTrue($"Expense type with name '{testName.Substring(0, Math.Min(50, testName.Length))}...' should fail");
            }
        }
    }
    
    [Fact]
    public async Task Should_HandleCircularDependencies_AndComplexRelationships()
    {
        // Test complex relationship scenarios that could cause issues
        
        // Arrange - Create base data
        ExpenseTypeDto expenseType = await Sender.CreateExpenseTypeAsync("Complex Expense Type");
        ExpenseDto expense = await Sender.CreateExpenseAsync(expenseType.Id, "Complex Expense");

        // Act - Try to create circular or complex dependencies
        
        // 1. Test deleting expense type that has expenses
        var deleteExpenseTypeCommand = new DeleteExpenseTypeCommand(expenseType.Id);
        Result deleteExpenseTypeResult = await Sender.Send(deleteExpenseTypeCommand);
        
        // This should either:
        // - Fail due to foreign key constraints (proper behavior)
        // - Succeed with cascade delete (also proper if designed that way)
        if (deleteExpenseTypeResult.IsFailure)
        {
            deleteExpenseTypeResult.Error.Type.Should().BeOneOf(ErrorType.Conflict, ErrorType.Validation);
            
            // Verify expense type still exists
            var getExpenseTypesQuery = new GetExpenseTypesQuery(null, null, 1, 100);
            Result<GetExpenseTypesQueryResponse> getExpenseTypesResult = await Sender.Send(getExpenseTypesQuery);
            getExpenseTypesResult.IsSuccess.Should().BeTrue();
            getExpenseTypesResult.Value.Items.Should().Contain(et => et.Id == expenseType.Id);
        }
        else
        {
            // If delete succeeded, verify cascade worked
            var getExpenseQuery = new GetExpenseQuery(expense.Id);
            Result<ExpenseDto> getExpenseResult = await Sender.Send(getExpenseQuery);
            getExpenseResult.IsFailure.Should().BeTrue("Expense should be deleted with its type");
        }

        // 2. Test duplicate name constraints across different contexts
        SettlementPeriodDto settlementPeriod1 = await Sender.CreateSettlementPeriodAsync(month: 1, year: 2024);
        SettlementPeriodDto settlementPeriod2 = await Sender.CreateSettlementPeriodAsync(month: 2, year: 2024);

        // Add income to both periods
        await Sender.AddIncomeToSettlementPeriodAsync(settlementPeriod1.Id, 2000);
        await Sender.AddIncomeToSettlementPeriodAsync(settlementPeriod2.Id, 2500);

        // Try to create another settlement period for the same month/year
        var duplicateSettlementCommand = new CreateSettlementPeriodCommand(1, 2024);
        Result<SettlementPeriodDto> duplicateResult = await Sender.Send(duplicateSettlementCommand);
        
        duplicateResult.IsFailure.Should().BeTrue("Duplicate settlement period should be rejected");
        duplicateResult.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Should_HandleTransactionalIntegrity_OnFailures()
    {
        // Test that partial failures don't leave the system in inconsistent states
        
        // Arrange - Create base scenario
        SettlementPeriodDto settlementPeriod = await Sender.CreateSettlementPeriodAsync();
        IncomeDto income = await Sender.AddIncomeToSettlementPeriodAsync(settlementPeriod.Id, 4000);
        
        // Act - Create a scenario that might fail partway through
        
        // 1. Try to remove income that doesn't exist
        var removeNonExistentIncomeCommand = new RemoveIncomeCommand(settlementPeriod.Id, "i_00000000-0000-0000-0000-000000000000");
        Result removeIncomeResult = await Sender.Send(removeNonExistentIncomeCommand);
        
        removeIncomeResult.IsFailure.Should().BeTrue("Removing non-existent income should fail");
        removeIncomeResult.Error.Type.Should().Be(ErrorType.NotFound);
        
        // Verify original data is unchanged
        var verifySettlementQuery = new GetSettlementPeriodQuery(settlementPeriod.Id);
        Result<SettlementPeriodDto> verifyResult = await Sender.Send(verifySettlementQuery);
        verifyResult.IsSuccess.Should().BeTrue();
        verifyResult.Value.TotalIncome.Should().Be(4000);
        verifyResult.Value.Incomes.Should().Contain(i => i.Id == income.Id);

        // 2. Try to add tax with non-existent tax type
        var addTaxWithInvalidTypeCommand = new AddTaxCommand(settlementPeriod.Id, 800, "tt_00000000-0000-0000-0000-000000000000");
        Result<TaxDto> addTaxResult = await Sender.Send(addTaxWithInvalidTypeCommand);
        
        addTaxResult.IsFailure.Should().BeTrue("Adding tax with invalid type should fail");
        addTaxResult.Error.Type.Should().Be(ErrorType.NotFound);
        
        // Verify settlement period is still in valid state
        var verifyAfterTaxQuery = new GetSettlementPeriodQuery(settlementPeriod.Id);
        Result<SettlementPeriodDto> verifyAfterTaxResult = await Sender.Send(verifyAfterTaxQuery);
        verifyAfterTaxResult.IsSuccess.Should().BeTrue();
        verifyAfterTaxResult.Value.TotalTaxes.Should().Be(0); // No taxes added
        verifyAfterTaxResult.Value.NetAmount.Should().Be(4000); // Still equals income

        // 3. Test expense creation with invalid expense type
        var createExpenseInvalidTypeCommand = new CreateExpenseCommand("Invalid Expense", "et_00000000-0000-0000-0000-000000000000", null);
        Result<ExpenseDto> createExpenseResult = await Sender.Send(createExpenseInvalidTypeCommand);
        
        createExpenseResult.IsFailure.Should().BeTrue("Creating expense with invalid type should fail");
        createExpenseResult.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Should_HandleUnicodeAndInternationalization_Properly()
    {
        // Test various international characters and Unicode scenarios
        (string name, string description)[] internationalTestCases = new[]
        {
            ("Café Revenue", "French"),
            ("Résumé Expenses", "French with accents"),
            ("Naïve Investment", "French with diaeresis"),
            ("Москва Tax", "Russian Cyrillic"),
            ("東京 Expense", "Japanese Kanji"),
            ("مصر Income", "Arabic"),
            ("🏦 Banking Fees", "Emoji"),
            ("Ñiño Fund", "Spanish"),
            ("Größe Cost", "German"),
            ("Παράδειγμα Goal", "Greek")
        };

        foreach ((string name, string description) in internationalTestCases)
        {
            // Test tax type creation with international names
            TaxTypeDto taxType = await Sender.CreateTaxTypeAsync(name);
            taxType.Should().NotBeNull();
            taxType.Name.Should().Be(name);

            // Test expense type creation
            ExpenseTypeDto expenseType = await Sender.CreateExpenseTypeAsync($"{name} Type");
            expenseType.Should().NotBeNull();
            expenseType.Name.Should().Be($"{name} Type");

            // Test financial goal creation
            FinancialGoalDto goal = await Sender.CreateFinancialGoalAsync(
                name: $"{name} Goal");
            
            goal.Should().NotBeNull();
            goal.Name.Should().Be($"{name} Goal");

            Console.WriteLine($"Successfully created entities with {description} characters");
        }
    }
}
