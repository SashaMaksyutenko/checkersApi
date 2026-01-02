using CheckersApi.Contracts;

namespace CheckersApi.Engine;

public interface IEngineAdapter
{
    SuggestResponse Suggest(SuggestRequest request, CancellationToken ct);
}
