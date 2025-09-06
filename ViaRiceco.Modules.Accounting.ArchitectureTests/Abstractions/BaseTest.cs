using System.Reflection;

namespace ViaRiceco.Modules.Accounting.ArchitectureTests.Abstractions;

#pragma warning disable CA1515
public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = typeof(Accounting.Application.AssemblyReference).Assembly;
    
    protected static readonly Assembly DomainAssembly = typeof(Accounting.Domain.AssemblyReference).Assembly;
    
    protected static readonly Assembly InfrastructureAssembly = typeof(Accounting.Infrastructure.AssemblyReference).Assembly;
    
    protected static readonly Assembly PresentationAssembly = typeof(Accounting.Presentation.AssemblyReference).Assembly;
}
