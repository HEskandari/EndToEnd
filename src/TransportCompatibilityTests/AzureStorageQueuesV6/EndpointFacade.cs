﻿namespace AzureStorageQueuesV6
{
    using System;
    using NServiceBus;
    using TransportCompatibilityTests.AzureStorageQueues;
    using TransportCompatibilityTests.Common;
    using TransportCompatibilityTests.Common.AzureStorageQueues;
    using TransportCompatibilityTests.Common.Messages;

    public class EndpointFacade : MarshalByRefObject, IEndpointFacade
    {
        private IBus bus;
        private MessageStore messageStore;
        private CallbackResultStore callbackResultStore;
        private SubscriptionStore subscriptionStore;

        public void Bootstrap(EndpointDefinition endpointDefinition)
        {
            InitializeEndpoint(endpointDefinition.As<AzureStorageQueuesEndpointDefinition>());
        }

        public void InitializeEndpoint(AzureStorageQueuesEndpointDefinition endpointDefinition)
        {
            var busConfiguration = new BusConfiguration();

            busConfiguration.Conventions()
                .DefiningMessagesAs(
                    t => t.Namespace != null && t.Namespace.EndsWith(".Messages") && t != typeof(TestEvent));
            busConfiguration.Conventions().DefiningEventsAs(t => t == typeof(TestEvent));

            busConfiguration.EndpointName(endpointDefinition.Name);
            busConfiguration.UsePersistence<InMemoryPersistence>();
            busConfiguration.EnableInstallers();
            busConfiguration.UseTransport<AzureStorageQueueTransport>().ConnectionString(AzureStorageQueuesConnectionStringBuilder.Build());

            busConfiguration.CustomConfigurationSource(new CustomConfiguration(endpointDefinition.Mappings));

            messageStore = new MessageStore();
            subscriptionStore = new SubscriptionStore();
            callbackResultStore = new CallbackResultStore();

            busConfiguration.RegisterComponents(c => c.RegisterSingleton(messageStore));

            var startableBus = Bus.Create(busConfiguration);

            bus = startableBus.Start();
        }

        public void SendCommand(Guid messageId)
        {
            bus.Send(new TestCommand { Id = messageId });
        }

        public void SendRequest(Guid requestId)
        {
            bus.Send(new TestRequest { RequestId = requestId });
        }

        public void PublishEvent(Guid eventId)
        {
            bus.Publish(new TestEvent { EventId = eventId });
        }

        public void SendAndCallbackForInt(int value)
        {
            throw new NotImplementedException();
        }

        public void SendAndCallbackForEnum(CallbackEnum value)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            bus.Dispose();
        }

        public Guid[] ReceivedMessageIds => messageStore.GetAll();

        public Guid[] ReceivedResponseIds => messageStore.Get<TestResponse>();

        public Guid[] ReceivedEventIds => messageStore.Get<TestEvent>();

        public int[] ReceivedIntCallbacks => callbackResultStore.Get<int>();

        public CallbackEnum[] ReceivedEnumCallbacks => callbackResultStore.Get<CallbackEnum>();

        public int NumberOfSubscriptions => subscriptionStore.NumberOfSubscriptions;
    }
}
