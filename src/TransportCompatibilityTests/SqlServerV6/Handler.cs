﻿using System.Threading.Tasks;
using NServiceBus;
using TransportCompatibilityTests.Common;
using TransportCompatibilityTests.Common.Messages;

namespace SqlServerV6
{
    public class Handler : IHandleMessages<TestCommand>, IHandleMessages<TestRequest>, IHandleMessages<TestResponse>, IHandleMessages<TestEvent>, IHandleMessages<TestIntCallback>, IHandleMessages<TestEnumCallback>
    {
        public MessageStore Store { get; set; }

        public Task Handle(TestCommand command, IMessageHandlerContext context)
        {
            Store.Add<TestCommand>(command.Id);

            return Task.FromResult(0);
        }

        public async Task Handle(TestRequest message, IMessageHandlerContext context)
        {
            await context.Reply(new TestResponse {ResponseId = message.RequestId});
        }

        public Task Handle(TestResponse message, IMessageHandlerContext context)
        {
            Store.Add<TestResponse>(message.ResponseId);

            return Task.FromResult(0);
        }

        public Task Handle(TestEvent message, IMessageHandlerContext context)
        {
            Store.Add<TestEvent>(message.EventId);

            return Task.FromResult(0);
        }

        public async Task Handle(TestIntCallback message, IMessageHandlerContext context)
        {
            await context.Reply(message.Response);
        }

        public async Task Handle(TestEnumCallback message, IMessageHandlerContext context)
        {
            await context.Reply(message.CallbackEnum);
        }
    }
}