﻿namespace TransportCompatibilityTests.RabbitMQ.Infrastructure
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using NUnit.Framework;
    using Common;
    using Common.RabbitMQ;

    [TestFixture]
    public abstract class RabbitMqContext
    {
        [SetUp]
        public void CommonSetUp()
        {
            if (string.IsNullOrWhiteSpace(RabbitConnectionStringBuilder.Build()))
            {
                throw new Exception($"Environment variables `{RabbitConnectionStringBuilder.EnvironmentVariable}`, `{RabbitConnectionStringBuilder.PermissionApiEnvironmentVariable}` and `{RabbitConnectionStringBuilder.VirtualHostApiEnvironmentVariable}` are required to connect to RabbitMQ.");
            }

            DeleteVirtualHost();
            CreateVirtualHost();
            SetupPermission();
        }

        static void DeleteVirtualHost()
        {
            ExecuteREST(client =>
            {
                var result = client.DeleteAsync(RabbitConnectionStringBuilder.VirtualHostAPI()).GetAwaiter().GetResult();
                return result.StatusCode == HttpStatusCode.NoContent || result.StatusCode == HttpStatusCode.NotFound;
            });
        }

        static void CreateVirtualHost()
        {
            ExecuteREST(client =>
            {
                var result = client.PutAsync(RabbitConnectionStringBuilder.VirtualHostAPI(), new StringContent(String.Empty, null, "application/json")).GetAwaiter().GetResult();
                return result.StatusCode == HttpStatusCode.NoContent || result.StatusCode == HttpStatusCode.Created;
            });
        }

        static void SetupPermission()
        {
            ExecuteREST(client =>
            {
                var result = client.PutAsync(RabbitConnectionStringBuilder.PermissionsAPI(), new StringContent(@"{""configure"":"".*"",""write"":"".*"",""read"":"".*""}", null, "application/json")).GetAwaiter().GetResult();
                return result.StatusCode == HttpStatusCode.NoContent;
            });
        }

        static void ExecuteREST(Func<HttpClient, bool> func)
        {
            using (var client = new HttpClient(new HttpClientHandler
            {
                Credentials = RabbitConnectionStringBuilder.Credentials()
            }, true)
            {
                Timeout = TimeSpan.FromSeconds(15),
            })
            {
                var attempts = 0;

                do
                {
                    if (func(client))
                    {
                        return;
                    }
                } while (attempts++ < 5);

                throw new Exception("Couldn't execute the REST call.");
            }
        }

        protected static object[][] GenerateVersionsPairs()
        {
            var versions = new[] { 1, 2, 3, 4, 5 };

            var pairs = from l in versions
                        from r in versions
                        from t in new[] {Topology.Direct, Topology.Convention}
                        where l != r
                        select new object[]
                        {
                            TransportVersions.RabbitMq(l), 
                            TransportVersions.RabbitMq(r), 
                            t
                        };

            return pairs.ToArray();
        }
    }
}
