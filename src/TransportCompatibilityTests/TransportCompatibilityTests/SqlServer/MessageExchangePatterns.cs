﻿using System;
using System.Linq;
using NUnit.Framework;
using TransportCompatibilityTests.Common;
using TransportCompatibilityTests.Common.Messages;


namespace TransportCompatibilityTests.SqlServer
{
    using Common.SqlServer;
    using MessageMapping = Common.SqlServer.MessageMapping;

    [TestFixture]
    public class MessageExchangePatterns : SqlServerContext
    {
        SqlServerEndpointDefinition sourceEndpointDefinition;
        SqlServerEndpointDefinition destinationEndpointDefinition;

        [SetUp]
        public void SetUp()
        {
            sourceEndpointDefinition = new SqlServerEndpointDefinition
            {
                Name = "Source"
            };
            destinationEndpointDefinition = new SqlServerEndpointDefinition
            {
                Name = "Destination"
            };
        }

        [Category("SqlServer")]
        [Test, TestCaseSource(nameof(GenerateVersionsPairs))]
        public void It_is_possible_to_send_command_between_different_versions(TransportVersion sourceVersion, TransportVersion destinationVersion)
        {
            sourceEndpointDefinition.Mappings = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestCommand),
                    TransportAddress = destinationEndpointDefinition.TransportAddressForVersion(destinationVersion)
                }
            };

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            {
                using (var destination = EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
                {
                    var messageId = Guid.NewGuid();

                    source.SendCommand(messageId);

                    AssertEx.WaitUntilIsTrue(() => destination.ReceivedMessageIds.Any(mi => mi == messageId));
                }
            }
        }

        [Category("SqlServer")]
        [Test, TestCaseSource(nameof(GenerateVersionsPairs))]
        public void It_is_possible_to_send_request_and_receive_replay(TransportVersion sourceVersion, TransportVersion destinationVersion)
        {
            sourceEndpointDefinition.Mappings = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestRequest),
                    TransportAddress = destinationEndpointDefinition.TransportAddressForVersion(destinationVersion)
                }
            };

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            {
                using (EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
                {
                    var requestId = Guid.NewGuid();

                    source.SendRequest(requestId);

                    AssertEx.WaitUntilIsTrue(() => source.ReceivedResponseIds.Any(responseId => responseId == requestId));
                }
            }
        }

        [Category("SqlServer")]
        [Test, TestCaseSource(nameof(GenerateMessageDrivenPubSubVersionsPairs))]
        public void It_is_possible_to_publish_events(TransportVersion sourceVersion, TransportVersion destinationVersion)
        {
            destinationEndpointDefinition.Publishers = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestEvent),
                    TransportAddress = sourceEndpointDefinition.TransportAddressForVersion(sourceVersion)
                }
            };

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            {
                using (var destination = EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
                {
                    AssertEx.WaitUntilIsTrue(() => source.NumberOfSubscriptions > 0);

                    var eventId = Guid.NewGuid();

                    source.PublishEvent(eventId);

                    AssertEx.WaitUntilIsTrue(() => destination.ReceivedEventIds.Any(ei => ei == eventId));
                }
            }
        }

        [Category("SqlServer")]
        [Test, TestCaseSource(nameof(GenerateVersionsPairs))]
        public void It_is_possible_to_send_and_receive_using_custom_schema_in_transport_address(TransportVersion sourceVersion, TransportVersion destinationVersion)
        {
            sourceEndpointDefinition.Schema = SourceSchema;
            sourceEndpointDefinition.Mappings = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestRequest),
                    TransportAddress = destinationEndpointDefinition.TransportAddressForVersion(destinationVersion),
                    Schema = DestinationSchema
                }
            };

            destinationEndpointDefinition.Schema = DestinationSchema;

            //TODO: this is a hack, passing mappings should be separate from passing schemas
            if (sourceVersion.Version == 2 && destinationVersion.Version == 3)
            {
                destinationEndpointDefinition.Mappings = new[]
                {
                    new MessageMapping
                    {
                        MessageType = typeof(TestResponse),
                        TransportAddress = sourceEndpointDefinition.TransportAddressForVersion(sourceVersion) + "." + Environment.MachineName,
                        Schema = SourceSchema
                    }
                };
            }
            else
            {
                destinationEndpointDefinition.Mappings = new[]
                {
                    new MessageMapping
                    {
                        MessageType = typeof(TestResponse),
                        TransportAddress = sourceEndpointDefinition.TransportAddressForVersion(sourceVersion),
                        Schema = SourceSchema
                    }
                };
            }

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            {
                using (EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
                {
                    var requestId = Guid.NewGuid();

                    source.SendRequest(requestId);

                    AssertEx.WaitUntilIsTrue(() => source.ReceivedResponseIds.Any());
                }
            }
        }

        static object[][] GenerateVersionsPairs()
        {
            var sqlTransportVersions = new[]
            {
                1,
                2,
                3,
                4,
                5,
                6,
            };

            var pairs = from l in sqlTransportVersions
                from r in sqlTransportVersions
                where l != r
                select new object[]
                {
                    TransportVersions.SqlTransportVersion(l),
                    TransportVersions.SqlTransportVersion(r),
                };

            return pairs.ToArray();
        }
        
        static object[][] GenerateMessageDrivenPubSubVersionsPairs()
        {
            var sqlTransportVersions = new[]
            {
                1,
                2,
                3,
                4,
                5,
                6,
            };

            var pairs = from l in sqlTransportVersions
                from r in sqlTransportVersions
                where l != r
                select new object[]
                {
                    TransportVersions.SqlTransportVersion(l),
                    TransportVersions.SqlTransportVersion(r),
                };

            return pairs.ToArray();
        }
    }
}