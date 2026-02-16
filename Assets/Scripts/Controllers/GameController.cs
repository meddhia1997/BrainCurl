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

    [Header("Layouts")]
    [SerializeField] private LayoutPresetSO defaultStartPreset;
    [SerializeField] private LayoutButton[] layoutButtons;

    [Header("Audio")]
    [SerializeField] private AudioConfigSO audioConfig;
    [SerializeField] private AudioPlayer audioPlayer;

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
    private PreviewService _previewService;

    private AudioService _audioService;

    private FlipBackTimerRunner _timerRunner;
    private DelayedEventRunner _delayedRunner;

    private SaveLoadService _saveLoad;
    private RestartService _restart;

    private bool _ended;
    private bool _previewRunning;

    private void Awake()
    {
        _bus = new EventBus();
        _saveLoad = new SaveLoadService();
        _restart = new RestartService(_saveLoad);

        _timerRunner = gameObject.AddComponent<FlipBackTimerRunner>();
        _timerRunner.Init(_bus);

        _delayedRunner = gameObject.AddComponent<DelayedEventRunner>();
        _delayedRunner.Init(_bus);

        if (audioPlayer == null)
            audioPlayer = GetComponent<AudioPlayer>();
        if (audioPlayer == null)
            audioPlayer = gameObject.AddComponent<AudioPlayer>();

        if (audioConfig != null)
        {
            if (audioConfig.IsValid(out var reason))
                _audioService = new AudioService(_bus, audioPlayer, audioConfig);
            else
                Debug.LogWarning($"[Audio] AudioConfigSO invalid: {reason}");
        }

        _bus.Subscribe<CardFlipRequested>(OnFlipRequested);
        _bus.Subscribe<CardFlipCompleted>(OnFlipCompleted);
        _bus.Subscribe<RestartRequested>(OnRestartRequested);
        _bus.Subscribe<ScoreChanged>(OnScoreChanged);
        _bus.Subscribe<AttemptsChanged>(OnAttemptsChanged);
        _bus.Subscribe<GameEnded>(OnGameEnded);
        _bus.Subscribe<PreviewEnded>(OnPreviewEnded);
        _bus.Subscribe<LayoutChangeRequested>(OnLayoutChangeRequested);

        if (restartButton != null) restartButton.Init(_bus);
        if (scoreHUD != null) scoreHUD.Init(_bus);
        if (attemptsHUD != null) attemptsHUD.Init(_bus);
        if (outcomeHUD != null) outcomeHUD.Init(_bus);

        if (layoutButtons != null)
        {
            for (int i = 0; i < layoutButtons.Length; i++)
            {
                if (layoutButtons[i] != null)
                    layoutButtons[i].Init(_bus);
            }
        }
    }

    private void Start()
    {
        if (rules == null)
        {
            Debug.LogError("GameRulesSO is missing on GameController.");
            return;
        }

        // Set starting preset for the board view
        if (defaultStartPreset != null)
            boardView.BuildBoard(defaultStartPreset, _bus, new BoardState(defaultStartPreset.Rows, defaultStartPreset.Cols, new int[defaultStartPreset.Rows * defaultStartPreset.Cols]), cardCatalog); // temporary build to set preset
        // We'll rebuild properly below.

        GameSaveData loaded = null;
        bool loadedOk = loadOnStart && _saveLoad.TryLoad(out loaded) && IsSaveCompatible(loaded);

        if (loadedOk)
        {
            // Use current boardView preset (whatever the scene has); save must match it
            _board = new BoardState(loaded.Rows, loaded.Cols, loaded.PairIds);
            loaded.ApplyTo(_board);

            CreateServicesForCurrentBoard();
            _scoreService.LoadState(loaded.Score, loaded.Combo);
            int tries = loaded.Version >= 2 ? loaded.TriesRemaining : rules.maxTries;
            _attemptsService.LoadState(tries);

            boardView.BuildBoard(boardView.CurrentPreset, _bus, _board, cardCatalog);
        }
        else
        {
            // Fresh new game using defaultStartPreset if assigned, else BoardView default
            var preset = defaultStartPreset != null ? defaultStartPreset : boardView.CurrentPreset;
            RebuildGame(preset, startPreview: true);
        }
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
            _bus.Unsubscribe<PreviewEnded>(OnPreviewEnded);
            _bus.Unsubscribe<LayoutChangeRequested>(OnLayoutChangeRequested);
        }

        DisposeServices();
        _audioService?.Dispose();
    }

    private void OnRestartRequested(RestartRequested _)
    {
        _restart.Restart(clearSave: true);
    }

    private void OnLayoutChangeRequested(LayoutChangeRequested evt)
    {
        if (evt.Preset == null) return;

        // New layout = new game state
        RebuildGame(evt.Preset, startPreview: true);
    }

    private void RebuildGame(LayoutPresetSO preset, bool startPreview)
    {
        _ended = false;
        _previewRunning = false;

        // Clear persistence because layout changed / new game
        _saveLoad.Delete();

        // Dispose old services (unsubscribe + stop timers)
        DisposeServices();

        // Validate: must be even
        int total = preset.Rows * preset.Cols;
        if ((total & 1) == 1)
        {
            Debug.LogError($"Invalid layout {preset.Rows}x{preset.Cols}: total cards must be even.");
            return;
        }

        // New board
        int seed = Environment.TickCount;
        _boardService = new BoardService(seed);
        _board = _boardService.Create(preset.Rows, preset.Cols);

        // New services
        CreateServicesForCurrentBoard();

        // Reset score & tries
        _scoreService.LoadState(0, 0);
        _attemptsService.LoadState(rules.maxTries);

        // Rebuild visuals
        boardView.BuildBoard(preset, _bus, _board, cardCatalog);

        // Optional preview: schedule next frame (animators ready)
        if (startPreview && rules.enablePreview && rules.previewSeconds > 0f)
        {
            _previewRunning = true;
            _previewService = new PreviewService(_bus, _board, _flipService, _delayedRunner, rules.previewSeconds);

            _delayedRunner.Schedule(0f, _ =>
            {
                if (!_ended)
                    _previewService.StartPreview();
            });
        }
    }

    private void CreateServicesForCurrentBoard()
    {
        _flipService = new FlipService(_board, _bus);
        _matchQueue = new MatchQueue(_board.TotalCards);
        _matchService = new MatchService(_board, _bus, _flipService, _timerRunner, mismatchDelaySeconds);

        _scoreService = new ScoreService(_bus, _board, matchBase, mismatchPenalty, winBonus);
        _attemptsService = new AttemptsService(_bus, rules.maxTries);
        _winService = new WinConditionService(_bus, _board);
        _endRestartService = new EndgameRestartService(_bus, _saveLoad, _delayedRunner, rules);
    }

    private void DisposeServices()
    {
        _matchService?.Dispose();
        _scoreService?.Dispose();
        _attemptsService?.Dispose();
        _winService?.Dispose();
        _endRestartService?.Dispose();

        _matchService = null;
        _scoreService = null;
        _attemptsService = null;
        _winService = null;
        _endRestartService = null;

        _matchQueue = null;
        _flipService = null;
        _boardService = null;
        _board = null;
    }

    private void OnGameEnded(GameEnded _)
    {
        _ended = true;
    }

    private void OnPreviewEnded(PreviewEnded _)
    {
        _previewRunning = false;
    }

    private void OnFlipRequested(CardFlipRequested evt)
    {
        if (_ended) return;
        if (_previewRunning) return;
        _flipService.TryFlipUp(evt.CardId);
    }

    private void OnFlipCompleted(CardFlipCompleted evt)
    {
        if (_ended) return;
        if (_previewRunning) return;
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
        if (data.PairIds == null) return false;

        // Save must match current boardview preset layout
        if (data.Rows != boardView.Rows || data.Cols != boardView.Cols) return false;
        if (data.PairIds.Length != boardView.TotalCards) return false;

        return true;
    }

    private void SaveSnapshot()
    {
        if (_board == null || _scoreService == null || _attemptsService == null) return;
        var data = GameSaveData.From(_board, _scoreService.Score, _scoreService.Combo, _attemptsService.TriesRemaining);
        _saveLoad.Save(data);
    }
}
