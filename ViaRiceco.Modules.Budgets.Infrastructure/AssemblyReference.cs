using System.Reflection;

namespace ViaRiceco.Modules.Budgets.Infrastructure;

public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;  
}

