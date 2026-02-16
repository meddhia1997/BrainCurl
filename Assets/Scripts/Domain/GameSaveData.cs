using System;
using UnityEngine;

[Serializable]
public sealed class GameSaveData
{
    public int Version = 2;

    public int Rows;
    public int Cols;

    public int[] PairIds;
    public bool[] Matched;

    public int Score;
    public int Combo;

    public int TriesRemaining;

    public long SavedAtUtcTicks;

    public static GameSaveData From(BoardState board, int score, int combo, int triesRemaining)
    {
        if (board == null) throw new ArgumentNullException(nameof(board));

        var data = new GameSaveData
        {
            Rows = board.Rows,
            Cols = board.Cols,
            PairIds = (int[])board.PairIds.Clone(),
            Matched = new bool[board.TotalCards],
            Score = score,
            Combo = combo,
            TriesRemaining = triesRemaining,
            SavedAtUtcTicks = DateTime.UtcNow.Ticks
        };

        for (int i = 0; i < board.States.Length; i++)
            data.Matched[i] = board.States[i] == CardState.Matched;

        return data;
    }

    public void ApplyTo(BoardState board)
    {
        if (board == null) throw new ArgumentNullException(nameof(board));

        for (int i = 0; i < board.States.Length; i++)
            board.States[i] = (Matched != null && i < Matched.Length && Matched[i]) ? CardState.Matched : CardState.FaceDown;
    }
}
