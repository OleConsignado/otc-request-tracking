using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Otc.RequestTracking.AspNetCore.Tests
{
    public class LoggerFactory : ILoggerFactory
    {
        private readonly LogEntryStore loggerBackStore;

        public LoggerFactory(LogEntryStore loggerBackStore)
        {
            this.loggerBackStore = loggerBackStore ?? throw new ArgumentNullException(nameof(loggerBackStore));
        }

        public void AddProvider(ILoggerProvider provider)
        {

        }

        public ILogger CreateLogger(string categoryName)
        {
            return new Logger(loggerBackStore);
        }

        public void Dispose()
        {

        }
    }
}
