public readonly struct CardFlipCompleted
{
    public readonly int CardId;
    public readonly bool IsFaceUp;

    public CardFlipCompleted(int cardId, bool isFaceUp)
    {
        CardId = cardId;
        IsFaceUp = isFaceUp;
    }
}
