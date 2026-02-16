using System;
using UnityEngine;
using Events;
public sealed class GameController : MonoBehaviour
{
    [SerializeField] private BoardView boardView;
    [SerializeField] private float mismatchDelaySeconds = 0.8f;

    [Header("Content")]
    [SerializeField] private CardCatalogSO cardCatalog;

    [Header("Rules")]
    [SerializeField] private GameRulesSO rules;

    [Header("UI")]
    [SerializeField] private RestartButton restartButton;
    [SerializeField] private ScoreHUD scoreHUD;
    [SerializeField] private AttemptsHUD attemptsHUD;
    [SerializeField] private OutcomeHUD outcomeHUD;

    [Header("Save/Load")]
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool autosave = true;

    [Header("Scoring")]
    [SerializeField] private int matchBase = 100;
    [SerializeField] private int mismatchPenalty = 20;
    [SerializeField] private int winBonus = 250;

    private IEventBus _bus;

    private BoardState _board;
    private BoardService _boardService;
    private FlipService _flipService;
    private MatchQueue _matchQueue;
    private MatchService _matchService;

    private ScoreService _scoreService;
    private AttemptsService _attemptsService;
    private WinConditionService _winService;
    private EndgameRestartService _endRestartService;

    private FlipBackTimerRunner _timerRunner;
    private DelayedEventRunner _delayedRunner;

    private SaveLoadService _saveLoad;
    private RestartService _restart;

    private bool _ended;

    private void Awake()
    {
        _bus = new EventBus();
        _saveLoad = new SaveLoadService();
        _restart = new RestartService(_saveLoad);

        _timerRunner = gameObject.AddComponent<FlipBackTimerRunner>();
        _timerRunner.Init(_bus);

        _delayedRunner = gameObject.AddComponent<DelayedEventRunner>();
        _delayedRunner.Init(_bus);

        _bus.Subscribe<CardFlipRequested>(OnFlipRequested);
        _bus.Subscribe<CardFlipCompleted>(OnFlipCompleted);
        _bus.Subscribe<RestartRequested>(OnRestartRequested);
        _bus.Subscribe<ScoreChanged>(OnScoreChanged);
        _bus.Subscribe<AttemptsChanged>(OnAttemptsChanged);
        _bus.Subscribe<GameEnded>(OnGameEnded);

        if (restartButton != null) restartButton.Init(_bus);
        if (scoreHUD != null) scoreHUD.Init(_bus);
        if (attemptsHUD != null) attemptsHUD.Init(_bus);
        if (outcomeHUD != null) outcomeHUD.Init(_bus);
    }

    private void Start()
    {
        if (rules == null)
        {
            Debug.LogError("GameRulesSO is missing on GameController.");
            return;
        }

        GameSaveData loaded = null;

        if (loadOnStart && _saveLoad.TryLoad(out loaded) && IsSaveCompatible(loaded))
        {
            _board = new BoardState(loaded.Rows, loaded.Cols, loaded.PairIds);
            loaded.ApplyTo(_board);
        }
        else
        {
            int seed = Environment.TickCount;
            _boardService = new BoardService(seed);
            _board = _boardService.Create(boardView.Rows, boardView.Cols);
        }

        _flipService = new FlipService(_board, _bus);
        _matchQueue = new MatchQueue(_board.TotalCards);
        _matchService = new MatchService(_board, _bus, _flipService, _timerRunner, mismatchDelaySeconds);

        _scoreService = new ScoreService(_bus, _board, matchBase, mismatchPenalty, winBonus);
        _attemptsService = new AttemptsService(_bus, rules.maxTries);
        _winService = new WinConditionService(_bus, _board);
        _endRestartService = new EndgameRestartService(_bus, _saveLoad, _delayedRunner, rules);

        // Load states (score/combo/tries)
        if (loaded != null)
        {
            _scoreService.LoadState(loaded.Score, loaded.Combo);

            // Version upgrade safety: if old save doesn't have tries, assume maxTries
            int tries = loaded.Version >= 2 ? loaded.TriesRemaining : rules.maxTries;
            _attemptsService.LoadState(tries);
        }
        else
        {
            _scoreService.LoadState(0, 0);
            _attemptsService.LoadState(rules.maxTries);
        }

        boardView.BuildBoard(_bus, _board, cardCatalog);
    }

    private void OnDestroy()
    {
        if (_bus != null)
        {
            _bus.Unsubscribe<CardFlipRequested>(OnFlipRequested);
            _bus.Unsubscribe<CardFlipCompleted>(OnFlipCompleted);
            _bus.Unsubscribe<RestartRequested>(OnRestartRequested);
            _bus.Unsubscribe<ScoreChanged>(OnScoreChanged);
            _bus.Unsubscribe<AttemptsChanged>(OnAttemptsChanged);
            _bus.Unsubscribe<GameEnded>(OnGameEnded);
        }

        _matchService?.Dispose();
        _scoreService?.Dispose();
        _attemptsService?.Dispose();
        _winService?.Dispose();
        _endRestartService?.Dispose();
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause) return;
        if (!autosave) return;
        if (_ended) return;
        SaveSnapshot();
    }

    private void OnApplicationQuit()
    {
        if (!autosave) return;
        if (_ended) return;
        SaveSnapshot();
    }

    private void OnRestartRequested(RestartRequested _)
    {
        _restart.Restart(clearSave: true);
    }

    private void OnGameEnded(GameEnded _)
    {
        _ended = true;
    }

    private void OnFlipRequested(CardFlipRequested evt)
    {
        if (_ended) return;
        _flipService.TryFlipUp(evt.CardId);
    }

    private void OnFlipCompleted(CardFlipCompleted evt)
    {
        if (_ended) return;
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

    private void OnScoreChanged(ScoreChanged _)
    {
        if (!autosave) return;
        if (_ended) return;
        SaveSnapshot();
    }

    private void OnAttemptsChanged(AttemptsChanged _)
    {
        if (!autosave) return;
        if (_ended) return;
        SaveSnapshot();
    }

    private bool IsSaveCompatible(GameSaveData data)
    {
        if (data == null) return false;
        if (data.Rows != boardView.Rows || data.Cols != boardView.Cols) return false;
        if (data.PairIds == null || data.PairIds.Length != boardView.TotalCards) return false;
        return true;
    }

    private void SaveSnapshot()
    {
        if (_board == null || _scoreService == null || _attemptsService == null) return;

        var data = GameSaveData.From(_board, _scoreService.Score, _scoreService.Combo, _attemptsService.TriesRemaining);
        _saveLoad.Save(data);
    }
}
