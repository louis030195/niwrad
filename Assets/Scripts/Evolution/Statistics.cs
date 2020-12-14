using System;
using System.Collections.Generic;
using System.Linq;
using Api.Realtime;
using UnityEngine;

namespace Evolution
{
    public class Statistics
    {
        private readonly List<ExperienceSample> _points = new List<ExperienceSample>();
        // private readonly List<float> _times = new List<float>(); // TODO: yet useless
        public event Action<(ExperienceSample p, float t)> Pushed;

        // private int _pushCount;
        // private const int SyncCount = 5;
        
        public void Push(ExperienceSample p)
        {
            _points.Add(p);
            // _times.Add(Time.time);
            Pushed?.Invoke((p, Time.time));
            // _pushCount++;
            // if (_pushCount % SyncCount == 0) 
        }

        public ExperienceSample Get() => _points.Last();
    }
}
