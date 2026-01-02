namespace CheckersApi.Validation;

public static class PdnNormalizer
{
    public static string Normalize(string pdn)
    {
        return pdn
            .Trim()
            .Replace(" ", "")
            .ToUpperInvariant();
    }

    public static string ToPositionKey(string pdn)
    {
        return $"pdn:{Normalize(pdn)}";
    }
}
