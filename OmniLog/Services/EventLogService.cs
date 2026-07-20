using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Threading.Tasks;
using OmniLog.Models;

namespace OmniLog.Services
{
    public class EventLogService
    {
        public Task<List<EventLogEntryModel>> GetEventLogsAsync(string logName, int maxEntries)
        {
            return Task.Run(() =>
            {
                var list = new List<EventLogEntryModel>();
                var query = new EventLogQuery(logName, PathType.LogName, "*")
                {
                    ReverseDirection = true // Read newest entries first
                };

                try
                {
                    using (var reader = new EventLogReader(query))
                    {
                        int count = 0;
                        EventRecord record;
                        while ((record = reader.ReadEvent()) != null && count < maxEntries)
                        {
                            using (record)
                            {
                                var model = new EventLogEntryModel
                                {
                                    RecordId = record.RecordId,
                                    EventId = record.Id,
                                    ProviderName = record.ProviderName,
                                    TimeCreated = record.TimeCreated,
                                    Level = record.Level,
                                    MachineName = record.MachineName,
                                    Channel = record.LogName,
                                    UserId = record.UserId?.Value ?? "N/A"
                                };

                                try
                                {
                                    model.Message = record.FormatDescription() ?? "No description available.";
                                }
                                catch (Exception ex)
                                {
                                    model.Message = $"Description cannot be loaded: {ex.Message}";
                                }

                                try
                                {
                                    model.Xml = record.ToXml();
                                }
                                catch
                                {
                                    model.Xml = string.Empty;
                                }

                                list.Add(model);
                                count++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Propagate core query errors (like Access Denied) back to the caller
                    throw new InvalidOperationException($"Error opening event log '{logName}': {ex.Message}", ex);
                }
                
                return list;
            });
        }
    }
}
