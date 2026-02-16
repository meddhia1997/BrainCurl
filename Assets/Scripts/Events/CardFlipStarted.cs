public readonly struct CardFlipStarted
{
    public readonly int CardId;
    public readonly bool ToFaceUp;

    public CardFlipStarted(int cardId, bool toFaceUp)
    {
        CardId = cardId;
        ToFaceUp = toFaceUp;
    }
}
