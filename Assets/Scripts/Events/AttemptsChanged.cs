public readonly struct AttemptsChanged
{
    public readonly int TriesRemaining;
    public readonly int TriesMax;

    public AttemptsChanged(int triesRemaining, int triesMax)
    {
        TriesRemaining = triesRemaining;
        TriesMax = triesMax;
    }
}
