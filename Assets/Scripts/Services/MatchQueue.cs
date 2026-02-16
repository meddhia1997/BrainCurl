public sealed class MatchQueue
{
    private readonly int[] _buf;
    private int _head;
    private int _tail;
    private int _count;

    public MatchQueue(int capacity)
    {
        _buf = new int[capacity];
        _head = 0;
        _tail = 0;
        _count = 0;
    }

    public void Enqueue(int cardId)
    {
        _buf[_tail] = cardId;
        _tail++;
        if (_tail == _buf.Length) _tail = 0;
        _count++;
    }

    public bool TryDequeue(out int cardId)
    {
        if (_count == 0)
        {
            cardId = -1;
            return false;
        }

        cardId = _buf[_head];
        _head++;
        if (_head == _buf.Length) _head = 0;
        _count--;
        return true;
    }

    public int Count => _count;
}
