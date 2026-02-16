public readonly struct CardInteractableChanged
{
    public readonly int CardId;
    public readonly bool Interactable;

    public CardInteractableChanged(int cardId, bool interactable)
    {
        CardId = cardId;
        Interactable = interactable;
    }
}
