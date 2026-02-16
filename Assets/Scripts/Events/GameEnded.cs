public readonly struct GameEnded
{
    public readonly bool IsWin;

    public GameEnded(bool isWin)
    {
        IsWin = isWin;
    }
}
