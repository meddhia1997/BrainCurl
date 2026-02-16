using System;

public sealed class BoardService
{
    private readonly Random _rng;

    public BoardService(int seed)
    {
        _rng = new Random(seed);
    }

    public BoardState Create(int rows, int cols)
    {
        int total = rows * cols;
        if ((total & 1) == 1)
            throw new ArgumentException("Board must have an even number of cards.");

        int pairs = total / 2;

        var pairIds = new int[total];
        int idx = 0;
        for (int p = 0; p < pairs; p++)
        {
            pairIds[idx++] = p;
            pairIds[idx++] = p;
        }

        // Fisher–Yates shuffle
        for (int i = pairIds.Length - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            int tmp = pairIds[i];
            pairIds[i] = pairIds[j];
            pairIds[j] = tmp;
        }

        return new BoardState(rows, cols, pairIds);
    }
}
