namespace CheckersApi.Engine;

public static class SearchLimitsResolver
{
    public static (int depth, int timeMs) Resolve(string level)
    {
        return level switch
        {
            "weak" => (8, 100),
            "medium" => (12, 250),
            "strong" => (16, 600),
            _ => (8, 100)
        };
    }
}
