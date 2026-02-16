using System;

public sealed class WinConditionService : IDisposable
{
    private readonly IEventBus _bus;
    private readonly BoardState _board;
    private bool _ended;

    public WinConditionService(IEventBus bus, BoardState board)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _board = board ?? throw new ArgumentNullException(nameof(board));

        _bus.Subscribe<PairResolved>(OnPairResolved);
        _bus.Subscribe<GameEnded>(OnGameEnded);
    }

    private void OnPairResolved(PairResolved evt)
    {
        if (_ended) return;
        if (!evt.IsMatch) return;

        if (IsBoardComplete())
            _bus.Publish(new GameEnded(isWin: true));
    }

    private bool IsBoardComplete()
    {
        var states = _board.States;
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i] != CardState.Matched)
                return false;
        }
        return true;
    }

    private void OnGameEnded(GameEnded _)
    {
        _ended = true;
    }

    public void Dispose()
    {
        _bus.Unsubscribe<PairResolved>(OnPairResolved);
        _bus.Unsubscribe<GameEnded>(OnGameEnded);
    }
}
