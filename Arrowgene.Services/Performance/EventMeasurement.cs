using System;

namespace Arrowgene.Services.Performance
{
    public class EventMeasurement
    {
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime Ended { get; set; }

        public EventMeasurement()
        {
        }
    }
}