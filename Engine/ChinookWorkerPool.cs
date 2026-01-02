namespace CheckersApi.Engine;

public sealed class ChinookWorkerPool
{
    private readonly ChinookWorker[] _workers;
    private int _index = -1;

    public ChinookWorkerPool(string exePath, int count)
    {
        _workers = Enumerable
            .Range(0, count)
            .Select(_ => new ChinookWorker(exePath))
            .ToArray();
    }

    public ChinookWorker Next()
    {
        var i = Interlocked.Increment(ref _index);
        return _workers[i % _workers.Length];
    }

    public int Count => _workers.Length;
}
