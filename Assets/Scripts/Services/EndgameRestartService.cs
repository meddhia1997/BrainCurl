using System;
using Events;

public sealed class EndgameRestartService : IDisposable
{
    private readonly IEventBus _bus;
    private readonly SaveLoadService _save;
    private readonly DelayedEventRunner _runner;
    private readonly GameRulesSO _rules;

    private bool _scheduled;

    public EndgameRestartService(IEventBus bus, SaveLoadService save, DelayedEventRunner runner, GameRulesSO rules)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _save = save ?? throw new ArgumentNullException(nameof(save));
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));
        _rules = rules ?? throw new ArgumentNullException(nameof(rules));

        _bus.Subscribe<GameEnded>(OnGameEnded);
    }

    private void OnGameEnded(GameEnded evt)
    {
        if (_scheduled) return;
        _scheduled = true;

        // Avoid reloading into an ended game
        _save.Delete();

        float delay = evt.IsWin ? _rules.winRestartDelaySeconds : _rules.defeatRestartDelaySeconds;

        _runner.Schedule(delay, bus => bus.Publish(new RestartRequested()));
    }

    public void Dispose()
    {
        _bus.Unsubscribe<GameEnded>(OnGameEnded);
    }
}
