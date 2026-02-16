using System;

public sealed class ScoreService : IDisposable
{
    private readonly IEventBus _bus;
    private readonly BoardState _board;

    private readonly int _matchBase;
    private readonly int _mismatchPenalty;
    private readonly int _winBonus;

    public int Score { get; private set; }
    public int Combo { get; private set; }

    public ScoreService(IEventBus bus, BoardState board, int matchBase = 100, int mismatchPenalty = 20, int winBonus = 250)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _board = board ?? throw new ArgumentNullException(nameof(board));

        _matchBase = matchBase;
        _mismatchPenalty = mismatchPenalty;
        _winBonus = winBonus;

        _bus.Subscribe<PairResolved>(OnPairResolved);
    }

    public void LoadState(int score, int combo)
    {
        Score = Math.Max(0, score);
        Combo = Math.Max(0, combo);

        _bus.Publish(new ScoreChanged(Score, Combo, 0));
    }

    private void OnPairResolved(PairResolved evt)
    {
        int delta = 0;

        if (evt.IsMatch)
        {
            Combo++;
            delta += _matchBase * Combo;
            Score += delta;

            // Win bonus (only when the board becomes complete)
            if (IsBoardComplete())
            {
                Score += _winBonus;
                delta += _winBonus;
            }
        }
        else
        {
            Combo = 0;
            Score -= _mismatchPenalty;
            if (Score < 0) Score = 0;
            delta = -_mismatchPenalty;
        }

        _bus.Publish(new ScoreChanged(Score, Combo, delta));
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

    public void Dispose()
    {
        _bus.Unsubscribe<PairResolved>(OnPairResolved);
    }
}
