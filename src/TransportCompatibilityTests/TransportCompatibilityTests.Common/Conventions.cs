namespace TransportCompatibilityTests.Common
{
    using System;
    using System.IO;
    using NUnit.Framework;

    public class Conventions
    {
        public static Func<EndpointDefinition, TransportVersion, string> AssemblyNameResolver =
            (definition, transportVersion) => $"{definition.TransportName}V{transportVersion.Version}";

        public static Func<EndpointDefinition, TransportVersion, string> AssemblyDirectoryResolver =
            (definition, transportVersion) =>
            {
                var configuration = "Release";

                #if DEBUG
                    configuration = "Debug";
                #endif

                var assemblyName = AssemblyNameResolver(definition, transportVersion);
                var bin = Path.Combine(TestContext.CurrentContext.TestDirectory, $"..\\..\\..\\..\\{assemblyName}\\bin\\{configuration}\\{transportVersion.TargetFramework}");
                if (Directory.Exists(bin))
                {
                    return bin;
                }

                throw new Exception($"Directory '{bin}' was not found");
            };

        public static Func<EndpointDefinition, TransportVersion, string> AssemblyPathResolver =
            (definition, transportVersion) =>
            {
                var assemblyName = AssemblyNameResolver(definition, transportVersion);
                var assemblyDirectory = new DirectoryInfo(AssemblyDirectoryResolver(definition, transportVersion));

                return Path.Combine(assemblyDirectory.FullName, assemblyName + ".dll");
            };

        public static Func<EndpointDefinition, TransportVersion, string> EndpointFacadeConfiguratorTypeNameResolver =
            (definition, transportVersion) =>
            {
                var assemblyName = AssemblyNameResolver(definition, transportVersion);

                return $"{assemblyName}.EndpointFacade";
            };
    }
}
