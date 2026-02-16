using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class DelayedEventRunner : MonoBehaviour
{
    private readonly List<Entry> _entries = new List<Entry>(8);
    private IEventBus _bus;

    private struct Entry
    {
        public float DueTime;
        public Action<IEventBus> Fire;
    }

    public void Init(IEventBus bus)
    {
        _bus = bus;
    }

    public void Schedule(float delaySeconds, Action<IEventBus> fire)
    {
        if (fire == null) return;

        _entries.Add(new Entry
        {
            DueTime = Time.unscaledTime + Mathf.Max(0f, delaySeconds),
            Fire = fire
        });
    }

    private void Update()
    {
        if (_entries.Count == 0 || _bus == null) return;

        float now = Time.unscaledTime;

        for (int i = _entries.Count - 1; i >= 0; i--)
        {
            if (now >= _entries[i].DueTime)
            {
                var fire = _entries[i].Fire;
                _entries.RemoveAt(i);
                fire(_bus);
            }
        }
    }
}
