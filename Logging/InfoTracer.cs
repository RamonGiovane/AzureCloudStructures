using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


/// <summary>
///     2020 - Version 0.5 
///     Developed by: Ramon Giovane Dias
/// </summary>
namespace Rgd.AzureAbstractions.Logging
{
    /// <summary>
    ///     Class able to mark points processed by the code, like the number of operations done by some service,
    ///     or get the execution time of the application.
    ///     The traces marked will be saved in a static data structure and can be retrieved as convetional string, or a html table,
    ///     or be printed with an ILogger instance.
    /// </summary>
    public class InfoTracer
    {
        private static DateTime _start = DateTime.Now;

        private static Stopwatch _watch;

        private static ConcurrentDictionary<string, int> _propertiesProcessed;

        private static InfoTracer _tracer;

        public static InfoTracer CreateOrGet()
        {
            if (_tracer == null) _tracer = new InfoTracer();
            return _tracer;
        }

        public double ExecutionTime { get => _watch.Elapsed.TotalSeconds; }

        private InfoTracer()
        {
            _propertiesProcessed = new ConcurrentDictionary<string, int>();
            _watch = new Stopwatch();
            _watch.Start();
        }

        public int GetPropertyValue(string propertyName)
        {
            return _propertiesProcessed.TryGetValue(propertyName, out int value) ? value : 0;
        }

        public void AddPropertyValue(string propertyName)
        {
            _propertiesProcessed.AddOrUpdate(propertyName, 1, (key, oldValue) => oldValue + 1);
        }

        public void SetPropertyValue(string propertyName, int value)
        {
            _propertiesProcessed.AddOrUpdate(propertyName, value, (key, oldValue) => oldValue = value);
        }

        public void Dispose()
        {
            _watch.Stop();
            _watch = null;
            _tracer = null;
        }

        public void LogReport(ILogger log)
        {

            log.LogCritical("EXEC. TIME: " + ExecutionTime + " seconds");

            log.LogCritical("Started at: " + _start.ToString() + "." + _start.Millisecond);

            foreach (KeyValuePair<string, int> e in _propertiesProcessed)
            {
                log.LogCritical(e.Key + " " + e.Value);
            }
        
        }

        public string ReportAsHtmlString()
        {
            StringBuilder report = new StringBuilder("<table border=\"1\">");

            foreach (KeyValuePair<string, int> e in _propertiesProcessed)
            {
                report.Append("<tr><td>").Append(e.Key).Append(":</td><td>").
                    Append(e.Value).Append("</td></tr>");
            }
            report.Append("<tr><td>EXEC. TIME:</td><td>").Append(ExecutionTime).Append(" seconds</td></tr>");
            report.Append("<tr><td>Started at:</td><td>").Append(_start.ToString()).Append(".").Append(_start.Millisecond).Append("</td></tr>");
            report.Append("</table>");
            
            return report.ToString();

        }


    }
}
