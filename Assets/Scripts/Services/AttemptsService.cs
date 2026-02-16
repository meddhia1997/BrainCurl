using System;

public sealed class AttemptsService : IDisposable
{
    private readonly IEventBus _bus;
    private readonly int _maxTries;

    public int TriesRemaining { get; private set; }
    public int TriesMax => _maxTries;

    public AttemptsService(IEventBus bus, int maxTries)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _maxTries = Math.Max(0, maxTries);

        TriesRemaining = _maxTries;

        _bus.Subscribe<PairResolved>(OnPairResolved);
    }

    public void LoadState(int triesRemaining)
    {
        TriesRemaining = Clamp(triesRemaining, 0, _maxTries);
        _bus.Publish(new AttemptsChanged(TriesRemaining, _maxTries));
    }

    private void OnPairResolved(PairResolved evt)
    {
        if (evt.IsMatch)
            return;

        if (TriesRemaining <= 0)
            return;

        TriesRemaining--;

        _bus.Publish(new AttemptsChanged(TriesRemaining, _maxTries));

        if (TriesRemaining <= 0)
            _bus.Publish(new GameEnded(isWin: false));
    }

    private static int Clamp(int v, int min, int max)
    {
        if (v < min) return min;
        if (v > max) return max;
        return v;
    }

    public void Dispose()
    {
        _bus.Unsubscribe<PairResolved>(OnPairResolved);
    }
}
