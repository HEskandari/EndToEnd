﻿namespace TransportCompatibilityTests.AzureStorageQueues
{
    using System;
    using System.Linq;
    using NUnit.Framework;
    using Common;
    using Common.AzureStorageQueues;
    using Common.Messages;

    [TestFixture]
    public class MessageExchangePatterns : AzureStorageQueuesContext
    {
        AzureStorageQueuesEndpointDefinition sourceEndpointDefinition;
        AzureStorageQueuesEndpointDefinition destinationEndpointDefinition;

        [SetUp]
        public void SetUp()
        {
            sourceEndpointDefinition = new AzureStorageQueuesEndpointDefinition { Name = "Source" };
            destinationEndpointDefinition = new AzureStorageQueuesEndpointDefinition { Name = "Destination" };
        }

        [Category("AzureStorageQueues")]
        [Test, TestCaseSource(nameof(GenerateVersionsPairs))]
        public void It_is_possible_to_send_command_between_different_versions(TransportVersion sourceVersion, TransportVersion destinationVersion)
        {
            sourceEndpointDefinition.Mappings = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestCommand),
                    TransportAddress = destinationEndpointDefinition.Name
                }
            };

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            using (var destination = EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
            {
                var messageId = Guid.NewGuid();

                source.SendCommand(messageId);

                AssertEx.WaitUntilIsTrue(() => destination.ReceivedMessageIds.Any(mi => mi == messageId));
            }
        }


        [Category("AzureStorageQueues")]
        [Test, TestCaseSource(nameof(GenerateVersionsPairs))]
        public void It_is_possible_to_send_request_and_receive_replay(TransportVersion sourceVersion, TransportVersion destinationVersion)
        {
            sourceEndpointDefinition.Mappings = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestRequest),
                    TransportAddress = sourceEndpointDefinition.Name
                }
            };

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            using (EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
            {
                var requestId = Guid.NewGuid();

                source.SendRequest(requestId);

                AssertEx.WaitUntilIsTrue(() => source.ReceivedResponseIds.Any(responseId => responseId == requestId));
            }
        }

        [Category("AzureStorageQueues")]
        [Test, TestCaseSource(nameof(GenerateVersionsPairs))]
        public void It_is_possible_to_publish_events(TransportVersion sourceVersion, TransportVersion destinationVersion)
        {
            destinationEndpointDefinition.Publishers = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestEvent),
                    TransportAddress = sourceEndpointDefinition.Name
                }
            };

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            using (var destination = EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
            {
                AssertEx.WaitUntilIsTrue(() => source.NumberOfSubscriptions > 0);

                var eventId = Guid.NewGuid();

                source.PublishEvent(eventId);

                AssertEx.WaitUntilIsTrue(() => destination.ReceivedEventIds.Any(ei => ei == eventId));
            }
        }


        static object[][] GenerateVersionsPairs()
        {
            var transportVersions = new[]
            {
                6,
                7,
                8
            };

            var pairs = from l in transportVersions
                        from r in transportVersions
                        where l != r
                        select new object[]
                        {
                            TransportVersions.AzureStorageQueues(l),
                            TransportVersions.AzureStorageQueues(r)
                        };

            return pairs.ToArray();
        }
    }
}
