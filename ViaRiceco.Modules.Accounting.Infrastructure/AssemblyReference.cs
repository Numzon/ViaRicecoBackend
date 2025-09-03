using System.Reflection;

namespace ViaRiceco.Modules.Accounting.Infrastructure;

public static class AssemblyReference
{
    public static Assembly Assembly => typeof(Domain.AssemblyReference).Assembly;  
}
