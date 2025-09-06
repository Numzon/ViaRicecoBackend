using System.Reflection;

namespace ViaRiceco.Modules.Portfolios.ArchitectureTests.Abstractions;

#pragma warning disable CA1515
public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = typeof(Portfolios.Application.AssemblyReference).Assembly;
    
    protected static readonly Assembly DomainAssembly = typeof(Portfolios.Domain.AssemblyReference).Assembly;
    
    protected static readonly Assembly InfrastructureAssembly = typeof(Portfolios.Infrastructure.AssemblyReference).Assembly;
    
    protected static readonly Assembly PresentationAssembly = typeof(Portfolios.Presentation.AssemblyReference).Assembly;
}
