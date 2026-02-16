using System;

public sealed class PreviewService
{
    private readonly IEventBus _bus;
    private readonly BoardState _board;
    private readonly FlipService _flip;
    private readonly DelayedEventRunner _runner;
    private readonly float _previewSeconds;

    public PreviewService(IEventBus bus, BoardState board, FlipService flip, DelayedEventRunner runner, float previewSeconds)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _board = board ?? throw new ArgumentNullException(nameof(board));
        _flip = flip ?? throw new ArgumentNullException(nameof(flip));
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));
        _previewSeconds = Math.Max(0f, previewSeconds);
    }

    public void StartPreview()
    {
        // Reveal all non-matched cards
        for (int i = 0; i < _board.TotalCards; i++)
        {
            if (_board.States[i] == CardState.FaceDown)
                _flip.TryFlipUp(i);
        }

        // After delay: flip them back down (only the ones that are face up)
        _runner.Schedule(_previewSeconds, bus =>
        {
            for (int i = 0; i < _board.TotalCards; i++)
            {
                if (_board.States[i] == CardState.FaceUp)
                {
                    // We need to mark them as Resolving to allow TryFlipDown with your FlipService rules.
                    // If your FlipService expects Resolving only, then we do it safely here:
                    _board.States[i] = CardState.Resolving;
                    _flip.TryFlipDown(i);
                }
            }

            bus.Publish(new PreviewEnded());
        });
    }
}
