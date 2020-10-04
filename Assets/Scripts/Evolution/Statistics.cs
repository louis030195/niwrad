using System;
using System.Collections.Generic;
using System.Linq;
using Api.Realtime;
using UnityEngine;

namespace Evolution
{
    public class Statistics
    {
        private readonly List<TimeSeriePoint> _points = new List<TimeSeriePoint>();
        // private readonly List<float> _times = new List<float>(); // TODO: yet useless
        public event Action<(TimeSeriePoint p, float t)> Pushed;
        
        public void Push(TimeSeriePoint p)
        {
            _points.Add(p);
            // _times.Add(Time.time);
            Pushed?.Invoke((p, Time.time));
        }

        public TimeSeriePoint Get() => _points.Last();
    }
}
