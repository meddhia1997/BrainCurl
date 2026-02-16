public readonly struct PairResolved
{
    public readonly int A;
    public readonly int B;
    public readonly bool IsMatch;

    public PairResolved(int a, int b, bool isMatch)
    {
        A = a;
        B = b;
        IsMatch = isMatch;
    }
}
