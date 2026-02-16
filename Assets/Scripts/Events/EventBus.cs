using System;
using System.Collections.Generic;

public sealed class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _map = new Dictionary<Type, List<Delegate>>(16);

    public void Subscribe<T>(Action<T> handler)
    {
        var t = typeof(T);
        if (!_map.TryGetValue(t, out var list))
        {
            list = new List<Delegate>(4);
            _map.Add(t, list);
        }
        list.Add(handler);
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        var t = typeof(T);
        if (_map.TryGetValue(t, out var list))
            list.Remove(handler);
    }

    public void Publish<T>(T evt)
    {
        var t = typeof(T);
        if (!_map.TryGetValue(t, out var list)) return;

        for (int i = 0; i < list.Count; i++)
            ((Action<T>)list[i])?.Invoke(evt);
    }
}
