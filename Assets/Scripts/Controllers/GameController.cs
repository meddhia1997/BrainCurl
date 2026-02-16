using UnityEngine;

public sealed class GameController : MonoBehaviour
{
    [SerializeField] private BoardView boardView;
    [SerializeField] private float mismatchDelaySeconds = 0.8f;

    [Header("Content")]
    [SerializeField] private CardCatalogSO cardCatalog;

    private IEventBus _bus;

    private BoardState _board;
    private BoardService _boardService;
    private FlipService _flipService;
    private MatchQueue _matchQueue;
    private MatchService _matchService;

    private FlipBackTimerRunner _timerRunner;

    private void Awake()
    {
        _bus = new EventBus();

        _timerRunner = gameObject.AddComponent<FlipBackTimerRunner>();
        _timerRunner.Init(_bus);

        _bus.Subscribe<CardFlipRequested>(OnFlipRequested);
        _bus.Subscribe<CardFlipCompleted>(OnFlipCompleted);
    }

    private void Start()
    {
        int seed = System.Environment.TickCount;

        _boardService = new BoardService(seed);
        _board = _boardService.Create(boardView.Rows, boardView.Cols);

        _flipService = new FlipService(_board, _bus);
        _matchQueue = new MatchQueue(_board.TotalCards);
        _matchService = new MatchService(_board, _bus, _flipService, _timerRunner, mismatchDelaySeconds);

        boardView.BuildBoard(_bus, _board, cardCatalog);
    }

    private void OnDestroy()
    {
        if (_bus != null)
        {
            _bus.Unsubscribe<CardFlipRequested>(OnFlipRequested);
            _bus.Unsubscribe<CardFlipCompleted>(OnFlipCompleted);
        }

        if (_matchService != null)
            _matchService.Dispose();
    }

    private void OnFlipRequested(CardFlipRequested evt)
    {
        _flipService.TryFlipUp(evt.CardId);
    }

    private void OnFlipCompleted(CardFlipCompleted evt)
    {
        if (!evt.IsFaceUp) return;

        _matchQueue.Enqueue(evt.CardId);

        while (_matchQueue.Count >= 2)
        {
            int a, b;
            if (!_matchQueue.TryDequeue(out a)) return;
            if (!_matchQueue.TryDequeue(out b)) return;

            _bus.Publish(new PairReady(a, b));
        }
    }
}
