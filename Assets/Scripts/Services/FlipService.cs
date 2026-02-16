public sealed class FlipService
{
    private readonly BoardState _board;
    private readonly IEventBus _bus;

    public FlipService(BoardState board, IEventBus bus)
    {
        _board = board;
        _bus = bus;
    }

    public bool TryFlipUp(int cardId)
    {
        var st = _board.States[cardId];
        if (st != CardState.FaceDown) return false;

        _board.States[cardId] = CardState.FaceUp;

        // lock only this card during animation
        _bus.Publish(new CardInteractableChanged(cardId, false));
        _bus.Publish(new CardFlipStarted(cardId, true));
        return true;
    }

    public bool TryFlipDown(int cardId)
    {
        var st = _board.States[cardId];
        if (st != CardState.Resolving) return false;

        _bus.Publish(new CardInteractableChanged(cardId, false));
        _bus.Publish(new CardFlipStarted(cardId, false));
        return true;
    }
}
