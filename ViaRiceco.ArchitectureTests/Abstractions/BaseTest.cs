namespace ViaRiceco.ArchitectureTests.Abstractions;

#pragma warning disable CA1515
public abstract class BaseTest
{
    protected const string AccountingNamespace = "ViaRiceco.Modules.Accounting";
    protected const string AccountingIntegrationEventsNamespace = "ViaRiceco.Modules.Accounting.IntegrationEvents";

    protected const string BudgetsNamespace = "ViaRiceco.Modules.Budgets";
    protected const string BudgetsIntegrationEventsNamespace = "ViaRiceco.Modules.Budgets.IntegrationEvents";

    protected const string PortfoliosNamespace = "ViaRiceco.Modules.Portfolios";
    protected const string PortfoliosIntegrationEventsNamespace = "ViaRiceco.Modules.Portfolios.IntegrationEvents";
}
#pragma warning restore CA1515
