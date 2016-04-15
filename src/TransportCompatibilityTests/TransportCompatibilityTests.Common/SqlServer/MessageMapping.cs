﻿namespace TransportCompatibilityTests.Common.SqlServer
{
    using System;

    [Serializable]
    public class MessageMapping
    {
        public Type MessageType { get; set; }
        public string TransportAddress { get; set; }
        public string Schema { get; set; }
    }
}
