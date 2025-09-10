using System.Reflection;

namespace ViaRiceco.Modules.Portfolios.Infrastructure;

public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;  
}
