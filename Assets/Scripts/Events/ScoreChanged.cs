public readonly struct ScoreChanged
{
    public readonly int Score;
    public readonly int Combo;
    public readonly int Delta;

    public ScoreChanged(int score, int combo, int delta)
    {
        Score = score;
        Combo = combo;
        Delta = delta;
    }
}
