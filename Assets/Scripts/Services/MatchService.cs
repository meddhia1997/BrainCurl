public sealed class MatchService
{
    private readonly BoardState _board;
    private readonly IEventBus _bus;
    private readonly FlipService _flipService;
    private readonly FlipBackTimerRunner _timer;
    private readonly float _mismatchDelaySeconds;

    public MatchService(BoardState board, IEventBus bus, FlipService flipService, FlipBackTimerRunner timer, float mismatchDelaySeconds)
    {
        _board = board;
        _bus = bus;
        _flipService = flipService;
        _timer = timer;
        _mismatchDelaySeconds = mismatchDelaySeconds;

        _bus.Subscribe<PairReady>(OnPairReady);
        _bus.Subscribe<FlipBackPairDue>(OnFlipBackDue);
        _bus.Subscribe<CardFlipCompleted>(OnFlipCompleted);
    }

    public void Dispose()
    {
        _bus.Unsubscribe<PairReady>(OnPairReady);
        _bus.Unsubscribe<FlipBackPairDue>(OnFlipBackDue);
        _bus.Unsubscribe<CardFlipCompleted>(OnFlipCompleted);
    }

    private void OnPairReady(PairReady evt)
    {
        int a = evt.A;
        int b = evt.B;

        if (_board.States[a] != CardState.FaceUp) return;
        if (_board.States[b] != CardState.FaceUp) return;

        bool isMatch = _board.PairIds[a] == _board.PairIds[b];

        if (isMatch)
        {
            _board.States[a] = CardState.Matched;
            _board.States[b] = CardState.Matched;

            _bus.Publish(new CardInteractableChanged(a, false));
            _bus.Publish(new CardInteractableChanged(b, false));
            _bus.Publish(new PairResolved(a, b, true));
            return;
        }

        // mismatch: mark resolving and schedule flip-back; do NOT block other flips
        _board.States[a] = CardState.Resolving;
        _board.States[b] = CardState.Resolving;

        _bus.Publish(new CardInteractableChanged(a, false));
        _bus.Publish(new CardInteractableChanged(b, false));
        _bus.Publish(new PairResolved(a, b, false));

        _timer.Schedule(_mismatchDelaySeconds, a, b);
    }

    private void OnFlipBackDue(FlipBackPairDue evt)
    {
        _flipService.TryFlipDown(evt.A);
        _flipService.TryFlipDown(evt.B);
    }

    private void OnFlipCompleted(CardFlipCompleted evt)
    {
        int id = evt.CardId;

        // when a resolving mismatch finishes flipping down, unlock it
        if (!evt.IsFaceUp && _board.States[id] == CardState.Resolving)
        {
            _board.States[id] = CardState.FaceDown;
            _bus.Publish(new CardInteractableChanged(id, true));
        }
    }
}
