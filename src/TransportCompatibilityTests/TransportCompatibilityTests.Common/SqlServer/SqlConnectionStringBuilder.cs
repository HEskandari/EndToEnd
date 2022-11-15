namespace TransportCompatibilityTests.Common.SqlServer
{
    using System;

    public class SqlServerConnectionStringBuilder
    {
        public static string EnvironmentVariable => "SqlServer_ConnectionString";

        public static string Build()
        {
            var value = Environment.GetEnvironmentVariable(EnvironmentVariable, EnvironmentVariableTarget.User);
            var v2 = Environment.GetEnvironmentVariable(EnvironmentVariable);
            return value ?? v2;
        }
    }
}
