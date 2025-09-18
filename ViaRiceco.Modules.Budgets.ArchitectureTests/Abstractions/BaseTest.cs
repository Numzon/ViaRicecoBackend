using System.Reflection;

namespace ViaRiceco.Modules.Budgets.ArchitectureTests.Abstractions;

#pragma warning disable CA1515
public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = typeof(Budgets.Application.AssemblyReference).Assembly;
    
    protected static readonly Assembly DomainAssembly = typeof(Budgets.Domain.AssemblyReference).Assembly;
    
    protected static readonly Assembly InfrastructureAssembly = typeof(Budgets.Infrastructure.AssemblyReference).Assembly;
    
    protected static readonly Assembly PresentationAssembly = typeof(Budgets.Presentation.AssemblyReference).Assembly;
}
