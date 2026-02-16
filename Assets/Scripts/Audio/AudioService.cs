using System;

public sealed class AudioService : IDisposable
{
    private readonly IEventBus _bus;
    private readonly AudioPlayer _player;
    private readonly AudioConfigSO _config;

    private bool _gameOverPlayed;

    public AudioService(IEventBus bus, AudioPlayer player, AudioConfigSO config)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _player = player ?? throw new ArgumentNullException(nameof(player));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        _bus.Subscribe<CardFlipStarted>(OnFlipStarted);
        _bus.Subscribe<PairResolved>(OnPairResolved);
        _bus.Subscribe<GameEnded>(OnGameEnded);
    }

    private void OnFlipStarted(CardFlipStarted _)
    {
        _player.PlayOneShot(_config.flip, _config.volume);
    }

    private void OnPairResolved(PairResolved evt)
    {
        _player.PlayOneShot(evt.IsMatch ? _config.match : _config.mismatch, _config.volume);
    }

    private void OnGameEnded(GameEnded _)
    {
        if (_gameOverPlayed) return;
        _gameOverPlayed = true;

        // Win/Defeat share the same required "game over" scenario clip
        _player.PlayOneShot(_config.gameOver, _config.volume);
    }

    public void Dispose()
    {
        _bus.Unsubscribe<CardFlipStarted>(OnFlipStarted);
        _bus.Unsubscribe<PairResolved>(OnPairResolved);
        _bus.Unsubscribe<GameEnded>(OnGameEnded);
    }
}
