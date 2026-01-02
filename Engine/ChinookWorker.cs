namespace CheckersApi.Engine;

public sealed class ChinookWorker
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ChinookProcess _engine;

    public ChinookWorker(string exePath)
    {
        _engine = new ChinookProcess(exePath);
    }

    public async Task<SearchResult> SearchAsync(
        string pdn,
        int depth,
        int timeMs,
        CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            _engine.SetPosition(pdn);
            return _engine.Search(depth, timeMs);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string> ProbeTablebaseAsync(string pdn, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _engine.ProbeTablebase(pdn);
        }
        finally
        {
            _lock.Release();
        }
    }
}
