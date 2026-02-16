using UnityEngine;

public sealed class GameController : MonoBehaviour
{
    [SerializeField] private BoardView boardView;

    private IEventBus _bus;
    private bool[] _faceUp; // temporary "no duplicate flips" gate for Sprint 2

    private void Awake()
    {
        _bus = new EventBus();
        _bus.Subscribe<CardFlipRequested>(OnFlipRequested);
        _bus.Subscribe<CardFlipCompleted>(OnFlipCompleted);
    }

    private void Start()
    {
        boardView.BuildBoard(_bus);

        _faceUp = new bool[boardView.TotalCards];
    }

    private void OnDestroy()
    {
        if (_bus == null) return;
        _bus.Unsubscribe<CardFlipRequested>(OnFlipRequested);
        _bus.Unsubscribe<CardFlipCompleted>(OnFlipCompleted);
    }

    private void OnFlipRequested(CardFlipRequested evt)
    {
        // Prevent duplicate flips (Sprint 2). Sprint 3 replaces this with FlipService + CardState.
        if (_faceUp[evt.CardId]) return;

        _faceUp[evt.CardId] = true;
        _bus.Publish(new CardFlipStarted(evt.CardId));
    }

    private void OnFlipCompleted(CardFlipCompleted evt)
    {
        // Sprint 2 stops here.
        // Sprint 3: enqueue evt.CardId to MatchQueue on completion.
    }
}
