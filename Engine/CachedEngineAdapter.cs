using CheckersApi.Contracts;
using CheckersApi.Validation;
using Microsoft.Extensions.Caching.Memory;

namespace CheckersApi.Engine;

public class CachedEngineAdapter : IEngineAdapter
{
    private readonly IEngineAdapter _inner;
    private readonly IMemoryCache _cache;

    public CachedEngineAdapter(IEngineAdapter inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public SuggestResponse Suggest(SuggestRequest request, CancellationToken ct)
    {
        var key = PdnNormalizer.ToPositionKey(request.State.Position);

        return _cache.GetOrCreate(key, entry =>
        {
            // за тз: 15 хвилин ttl
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

            // priority для lru eviction
            entry.SetPriority(CacheItemPriority.Normal);

            // кожна позиція = 1 (щоб було max 20k позицій)
            entry.SetSize(1);

            return _inner.Suggest(request, ct);
        })!;
    }
}
