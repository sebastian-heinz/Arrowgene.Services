using System;
using System.Collections.Generic;
using System.Diagnostics;
using Arrowgene.Services.Logging;

namespace Arrowgene.Services.Performance
{
    public class PerformanceMonitor
    {
        public static readonly PerformanceMonitor Instance = new PerformanceMonitor();

        private readonly Process _process;
        private readonly ILogger _logger;
        private readonly Dictionary<string, Stopwatch> _watches;
        private readonly Dictionary<string, List<EventMeasurement>> _events;
        private readonly List<CpuMeasurement> _cpuMeasurements;
        private readonly List<MemoryMeasurement> _memoryMeasurements;
        private readonly object _eventLock;

        private double _cpuTimeOld = 0;
        private DateTime _cpuStampOld = DateTime.Now;
        private double _cpuNewTime = 0;
        private DateTime _cpuNewStamp = DateTime.Now;
        private double _cpuChange = 0;
        private double _cpuPeriod = 0;

        public OperatingSystem Os { get; }
        public int ProcessorCount { get; }
        public bool Is64BitOperatingSystem { get; }
        public bool Is64BitProcess { get; }
        public Version NetVersion { get; }
        public string CurrentDirectory { get; }
        public string MachineName { get; }
        public string UserName { get; }
        public string SystemDirectory { get; }

        public bool StopwatchIsHighResolution { get; }

        public PerformanceMonitor()
        {
            _logger = LogProvider.Logger(this);
            _eventLock = new object();
            _watches = new Dictionary<string, Stopwatch>();
            _events = new Dictionary<string, List<EventMeasurement>>();
            _cpuMeasurements = new List<CpuMeasurement>();
            _memoryMeasurements = new List<MemoryMeasurement>();
            _process = Process.GetCurrentProcess();
            Os = Environment.OSVersion;
            ProcessorCount = Environment.ProcessorCount;
            Is64BitOperatingSystem = Environment.Is64BitOperatingSystem;
            NetVersion = Environment.Version;
            Is64BitProcess = Environment.Is64BitProcess;
            CurrentDirectory = Environment.CurrentDirectory;
            MachineName = Environment.MachineName;
            UserName = Environment.UserName;
            SystemDirectory = Environment.SystemDirectory;
            StopwatchIsHighResolution = Stopwatch.IsHighResolution;
        }

        public Dictionary<string, List<EventMeasurement>> GetEvents()
        {
            return new Dictionary<string, List<EventMeasurement>>(_events);
        }

        public EventMeasurement GetLatest(string name)
        {
            //  return _events[name][0];
            return new EventMeasurement();
        }

        public TimeSpan GetAverage(string name)
        {
            return new TimeSpan();
        }

        public void StartEvent(string name)
        {
            lock (_eventLock)
            {
                if (_watches.ContainsKey(name))
                {
                    _logger.Error($"Stopwatch for name: {name} is still active.");
                    return;
                }

                Stopwatch stopWatch = Stopwatch.StartNew();
                _watches.Add(name, stopWatch);
            }
        }

        public void StopEvent(string name)
        {
            Stopwatch stopWatch;
            lock (_eventLock)
            {
                if (!_watches.ContainsKey(name))
                {
                    _logger.Error($"Stopwatch for name: {name} is not started.");
                    return;
                }

                stopWatch = _watches[name];
                stopWatch.Stop();
                _watches.Remove(name);
            }

            EventMeasurement eventMeasurement = new EventMeasurement();
            eventMeasurement.Name = name;
            eventMeasurement.Duration = stopWatch.Elapsed;
            eventMeasurement.Ended = DateTime.Now;

            List<EventMeasurement> measurements;
            if (_events.ContainsKey(name))
            {
                measurements = _events[name];
            }
            else
            {
                measurements = new List<EventMeasurement>();
                _events.Add(name, measurements);
            }

            measurements.Add(eventMeasurement);
        }

        public void Measure()
        {
            MeasureCpuUsage();
            MeasureMemoryUsage();
        }

        public void MeasureCpuUsage()
        {
            _process.Refresh();
            _cpuNewTime = _process.TotalProcessorTime.TotalMilliseconds;
            _cpuNewStamp = DateTime.Now;
            _cpuChange = _cpuNewTime - _cpuTimeOld;
            _cpuPeriod = _cpuNewStamp.Subtract(_cpuStampOld).TotalMilliseconds;
            _cpuTimeOld = _cpuNewTime;
            _cpuStampOld = _cpuNewStamp;
            double use = (_cpuChange / (_cpuPeriod * ProcessorCount) * 100.0);
            if (use > 100.0)
            {
                use = 100.0;
            }

            CpuMeasurement cpuMeasurement = new CpuMeasurement();
            cpuMeasurement.Time = DateTime.Now;
            cpuMeasurement.Usage = use;
            _cpuMeasurements.Add(cpuMeasurement);
        }

        public void MeasureMemoryUsage()
        {
            _process.Refresh();
            MemoryMeasurement memoryMeasurement = new MemoryMeasurement();
            memoryMeasurement.Time = DateTime.Now;
            memoryMeasurement.Usage = _process.WorkingSet64;
            _memoryMeasurements.Add(memoryMeasurement);
        }
    }
}