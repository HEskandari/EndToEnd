﻿namespace TransportCompatibilityTests.Common
{
    using System;
    using AzureServiceBus;
    using RabbitMQ;
    using SqlServer;

    public static class ExtensionMethods
    {
        public static T As<T>(this object target)
        {
            return (T) target;
        }

        public static string TransportAddressForVersion(this RabbitMqEndpointDefinition endpointDefinition, TransportVersion transportVersion)
        {
            return (transportVersion.Version < 4) ? endpointDefinition.Name + "." + Environment.MachineName : endpointDefinition.Name;
        }

        public static string TransportAddressForVersion(this SqlServerEndpointDefinition endpointDefinition, TransportVersion transportVersion)
        {
            return transportVersion.Version == 1 ? endpointDefinition.Name + "." + Environment.MachineName : endpointDefinition.Name;
        }
        public static string TransportAddressForVersion(this AzureServiceBusEndpointDefinition endpointDefinition, TransportVersion version)
        {
            return endpointDefinition.Name;
        }
    }
}
