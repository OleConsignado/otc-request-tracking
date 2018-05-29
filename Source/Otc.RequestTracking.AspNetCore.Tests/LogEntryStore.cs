namespace Otc.RequestTracking.AspNetCore.Tests
{
    public class LogEntryStore
    {
        public LogModel LogEntry { get; set; }

        public void Clear()
        {
            LogEntry = null;
        }
    }
}