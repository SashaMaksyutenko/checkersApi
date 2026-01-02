namespace CheckersApi.Validation;

public static class PdnUtils
{
    public static int CountPieces(string pdn)
    {
        return pdn
            .Split(':')
            .Where(p => p.StartsWith("W") || p.StartsWith("B"))
            .SelectMany(p => p[1..].Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Count();
    }
}
