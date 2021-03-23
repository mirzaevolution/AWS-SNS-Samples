using System;

namespace SNSModels
{
    public class EventMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Message { get; set; }
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    }
}
