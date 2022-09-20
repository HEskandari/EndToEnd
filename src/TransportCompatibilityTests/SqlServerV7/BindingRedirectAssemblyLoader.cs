using System;
using System.IO;
using System.Reflection;

namespace SqlServerV7;

public class BindingRedirectAssemblyLoader
{
    public static Assembly CurrentDomain_BindingRedirect(object sender, ResolveEventArgs args)
    {
        var dir = AppDomain.CurrentDomain.BaseDirectory;
        var name = new AssemblyName(args.Name);
        switch (name.Name)
        {
            case "System.Runtime.CompilerServices.Unsafe":
                return Assembly.LoadFrom(Path.Combine(dir, "System.Runtime.CompilerServices.Unsafe.dll"));

            default:
                return null;
        }
    }
}