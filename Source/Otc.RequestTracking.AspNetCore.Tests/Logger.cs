using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Otc.RequestTracking.AspNetCore.Tests
{
    public class Logger : ILogger
    {
        private LogEntryStore loggerBackStore;

        public Logger(LogEntryStore loggerBackStore)
        {
            this.loggerBackStore = loggerBackStore ?? throw new ArgumentNullException(nameof(loggerBackStore));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new FakeLogScope();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if(eventId.Id == RequestTracker.RequestLogEventId)
            {
                loggerBackStore.LogEntry = (state as FormattedLogValues)?[0].Value as LogModel;
            }
        }
    }
}
