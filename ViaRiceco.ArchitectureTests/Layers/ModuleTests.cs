using System.Reflection;
using NetArchTest.Rules;
using ViaRiceco.ArchitectureTests.Abstractions;

namespace ViaRiceco.ArchitectureTests.Layers;

public class ModuleTests : BaseTest
{
    [Fact]
    public void BudgetsModule_ShouldNotHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules = [AccountingNamespace, PortfoliosNamespace];
        string[] integrationEventsModules = [
            AccountingIntegrationEventsNamespace,
            PortfoliosIntegrationEventsNamespace];

        List<Assembly> budgetsAssemblies =
        [
            Modules.Budgets.Domain.AssemblyReference.Assembly,
            Modules.Budgets.Application.AssemblyReference.Assembly,
            Modules.Budgets.Presentation.AssemblyReference.Assembly,
            Modules.Budgets.Infrastructure.AssemblyReference.Assembly
        ];

        Types.InAssemblies(budgetsAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void AccountingModule_ShouldNotHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules = [BudgetsNamespace, PortfoliosNamespace];
        string[] integrationEventsModules = [
            BudgetsIntegrationEventsNamespace,
            PortfoliosIntegrationEventsNamespace];

        List<Assembly> eventsAssemblies =
        [
            Modules.Accounting.Domain.AssemblyReference.Assembly,
            Modules.Accounting.Application.AssemblyReference.Assembly,
            Modules.Accounting.Presentation.AssemblyReference.Assembly,
            Modules.Accounting.Infrastructure.AssemblyReference.Assembly
        ];

        Types.InAssemblies(eventsAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void PortfoliosModule_ShouldNotHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules = [AccountingNamespace, BudgetsNamespace];
        string[] integrationEventsModules = [
            AccountingIntegrationEventsNamespace,
            BudgetsIntegrationEventsNamespace];

        List<Assembly> ticketingAssemblies =
        [
            Modules.Portfolios.Domain.AssemblyReference.Assembly,
            Modules.Portfolios.Application.AssemblyReference.Assembly,
            Modules.Portfolios.Presentation.AssemblyReference.Assembly,
            Modules.Portfolios.Infrastructure.AssemblyReference.Assembly
        ];

        Types.InAssemblies(ticketingAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }
}
