using System.Collections.Generic;
using System.Text;

namespace Arrowgene.Services.Performance
{
    public class PerformanceReporter
    {
        private PerformanceMonitor _monitor;

        public PerformanceReporter(PerformanceMonitor monitor)
        {
            _monitor = monitor;
        }

        public string GetSystemReport()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"OS: {_monitor.Os}");
            sb.AppendLine($"Processors: {_monitor.ProcessorCount}");
            sb.AppendLine($"x64 OS: {_monitor.Is64BitOperatingSystem}");
            sb.AppendLine($"x64 Process: {_monitor.Is64BitProcess}");
            sb.AppendLine($".NET Version: {_monitor.NetVersion}");
            sb.AppendLine($"CurrentDirectory: {_monitor.CurrentDirectory}");
            sb.AppendLine($"SystemDirectory: {_monitor.SystemDirectory}");
            sb.AppendLine($"Machine Name: {_monitor.MachineName}");
            sb.AppendLine($"User Name: {_monitor.UserName}");
            sb.AppendLine($"Stopwatch IsHighResolution: {_monitor.StopwatchIsHighResolution}");
            sb.AppendLine();
            return sb.ToString();
        }

        public string GetEventReports(int maxHistory = 10)
        {
            Dictionary<string, List<EventMeasurement>> events = _monitor.GetEvents();
            StringBuilder sb = new StringBuilder();
            if (events.Count > 0)
            {
                sb.AppendLine("Event Measurements - Start");

                foreach (string key in events.Keys)
                {
                    List<EventMeasurement> measurements = events[key];
                    if (measurements.Count <= 0)
                    {
                        continue;
                    }

                    sb.AppendLine("---");
                    sb.AppendLine($"Event Measurement: {key}");

                    if (measurements.Count < maxHistory)
                    {
                        maxHistory = measurements.Count;
                    }

                    int index = measurements.Count - maxHistory;
                    int count = maxHistory;
                    List<EventMeasurement> parts = measurements.GetRange(index, count);
                    parts.Reverse();
                    double totalSum = 0;
                    foreach (EventMeasurement part in parts)
                    {
                        sb.AppendLine(
                            $"Event: {part.Name} Duration: {part.Duration.TotalMilliseconds} MS ({part.Duration.TotalSeconds} S) Completed: {part.Ended}");
                        totalSum += part.Duration.TotalMilliseconds;
                    }

                    sb.AppendLine($"Average: {totalSum / maxHistory} MS");
                    sb.AppendLine("---");
                }

                sb.AppendLine("Event Measurements - End");
            }

            sb.AppendLine();
            return sb.ToString();
        }
    }
}