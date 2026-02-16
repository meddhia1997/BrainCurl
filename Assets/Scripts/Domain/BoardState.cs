public sealed class BoardState
{
    public readonly int Rows;
    public readonly int Cols;

    // fixed-size arrays for perf
    public readonly int[] PairIds;      // len = Rows*Cols
    public readonly CardState[] States; // len = Rows*Cols

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
}
