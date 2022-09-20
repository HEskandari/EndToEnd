using System;
using System.Linq;
using NUnit.Framework;
using TransportCompatibilityTests.Common;
using TransportCompatibilityTests.Common.SqlServer;

namespace TransportCompatibilityTests.SqlServer
{
    [TestFixture]
    public class NativePubSub
    {
        SqlServerEndpointDefinition subscriberDefinition;
        SqlServerEndpointDefinition publisherDefinition;

        [SetUp]
        public void SetUp()
        {
            subscriberDefinition = new SqlServerEndpointDefinition
            {
                Name = "Subscriber",
                UseNativePubSub = true,
            };
            publisherDefinition = new SqlServerEndpointDefinition
            {
                Name = "Publisher",
                UseNativePubSub = true
            };
        }

        [Category("SqlServer")]
        [Test]
        public void It_is_possible_to_natively_publish_events()
        {
            using (var subscriber = EndpointFacadeBuilder.CreateAndConfigure(subscriberDefinition, TransportVersions.SqlTransportVersion(7)))
            using (var publisher = EndpointFacadeBuilder.CreateAndConfigure(publisherDefinition, TransportVersions.SqlTransportVersion(6)))
            {
                var eventId = Guid.NewGuid();

                publisher.PublishEvent(eventId);

                AssertEx.WaitUntilIsTrue(() => subscriber.ReceivedEventIds.Any(ei => ei == eventId));
                Assert.AreEqual(1, subscriber.ReceivedEventIds.Length);
            }
        }
    }
}