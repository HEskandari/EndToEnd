namespace TransportCompatibilityTests.Common;

public class TransportVersion
{
    public int Version { get; set; }
    public string TargetFramework { get; set; }

    public override string ToString()
    {
        return $"v{Version}";
    }
}

public static class TransportVersions
{
    public static TransportVersion SqlTransportVersion(int version)
    {
        return new TransportVersion
        {
            Version = version,
            TargetFramework = version <= 4 ? "net452" : "net48"
        };
    }

    public static TransportVersion RabbitMq(int version)
    {
        return new TransportVersion
        {
            Version = version,
            TargetFramework = "net452"
        };
    }

    public static TransportVersion AzureStorageQueues(int version)
    {
        return new TransportVersion
        {
            Version = version,
            TargetFramework = "net452"
        };
    }

    public static TransportVersion AzureServiceBus(int version)
    {
        return new TransportVersion
        {
            Version = version,
            TargetFramework = "net452"
        };
    }

    public static TransportVersion AmazonSQS(int version)
    {
        return new TransportVersion
        {
            Version = version,
            TargetFramework = "net452"
        };
    }
}