public sealed class BoardState
{
    public readonly int Rows;
    public readonly int Cols;

    // Pair identity per card (matching uses this)
    public readonly int[] PairIds;

    // Runtime state per card
    public readonly CardState[] States;

    public int TotalCards => PairIds.Length;

    public BoardState(int rows, int cols, int[] pairIds)
    {
        Rows = rows;
        Cols = cols;
        PairIds = pairIds;

        States = new CardState[pairIds.Length];
        for (int i = 0; i < States.Length; i++)
            States[i] = CardState.FaceDown;
    }

    public int GetPairId(int cardId) => PairIds[cardId];
}
