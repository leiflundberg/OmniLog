using System;

namespace OmniLog.Models
{
    public class EventLogEntryModel
    {
        public long? RecordId { get; set; }
        public int EventId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public byte? Level { get; set; }
        public DateTime? TimeCreated { get; set; }
        public string Message { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Xml { get; set; } = string.Empty;
    }
}
