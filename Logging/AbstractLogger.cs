using Microsoft.Extensions.Logging;
using System;

/// <summary>
///     2020 - Version 1.2 
///     Developed by: Ramon Giovane Dias
/// </summary>
namespace Rgd.AzureAbstractions.Logging
{
    /// <summary>
    ///  A Logger class that should be inherited by other service classes, in order to trace their operations using ILogger.
    ///  If the ILogger instance is null the logging messages will be printed on the local console.
    ///  If the ILogger instance is not null but somehow invalid or the service is disabled or misconfigured, the messages will not be printed anywhere.
    /// </summary>
    public abstract class AbstractLogger
    {

        private ILogger Logger { get; set; }

        /// <summary>
        ///    Set this flag as true to ignore and don't show logging messages
        /// </summary>
        public bool LoggingDisabled { get; set; }

        protected InfoTracer Tracer { get => InfoTracer.CreateOrGet(); }

        public AbstractLogger(ILogger logger)
        {
            Logger = logger;

        }

        public AbstractLogger() : this(null) { }

        protected void TryLog(string msg)
        {
            if (LoggingDisabled) return;

            if (Logger == null) ConsoleLog(msg);
            else Logger.LogCritical(msg);
        }
        protected void TryLogError(string msg)
        {
            if (LoggingDisabled) return;

            if (Logger == null) ConsoleLog(msg);
            else Logger.LogError(msg);
        }
        protected void ConsoleLog(string msg)
        {
             Console.WriteLine(msg);
        }

        


    }
}
