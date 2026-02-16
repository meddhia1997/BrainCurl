using UnityEngine;

public sealed class FlipBackTimerRunner : MonoBehaviour
{
    private struct Pending
    {
        public float Remaining;
        public int A;
        public int B;
    }

    [SerializeField] private int initialCapacity = 16;

    private Pending[] _items;
    private int _count;
    private IEventBus _bus;

    public void Init(IEventBus bus)
    {
        _bus = bus;
        _items = new Pending[Mathf.Max(4, initialCapacity)];
        _count = 0;
    }

    public void Schedule(float delaySeconds, int a, int b)
    {
        if (_count == _items.Length)
            Grow();

        _items[_count++] = new Pending { Remaining = delaySeconds, A = a, B = b };
    }

    private void Grow()
    {
        var next = new Pending[_items.Length * 2];
        for (int i = 0; i < _items.Length; i++)
            next[i] = _items[i];
        _items = next;
    }

    private void Update()
    {
        if (_count == 0 || _bus == null) return;

        float dt = Time.deltaTime;

        int write = 0;
        for (int read = 0; read < _count; read++)
        {
            var p = _items[read];
            p.Remaining -= dt;

            if (p.Remaining <= 0f)
            {
                _bus.Publish(new FlipBackPairDue(p.A, p.B));
                continue;
            }

            _items[write++] = p;
        }

        _count = write;
    }
}
