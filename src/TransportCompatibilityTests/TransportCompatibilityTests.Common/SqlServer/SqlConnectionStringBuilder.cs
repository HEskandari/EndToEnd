namespace TransportCompatibilityTests.Common.SqlServer
{
    using System;

    public class SqlServerConnectionStringBuilder
    {
        public static string EnvironmentVariable => "SqlServer_ConnectionString";

        public static string Build()
        {
            var value = Environment.GetEnvironmentVariable(EnvironmentVariable, EnvironmentVariableTarget.User);
            return value ?? Environment.GetEnvironmentVariable(EnvironmentVariable);
        }
    }
}
