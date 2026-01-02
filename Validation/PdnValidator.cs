//using System.Text.RegularExpressions;

//namespace CheckersApi.Validation;

//public static class PdnValidator
//{
//    // Дозволяє:
//    // - W:W31,W32,W33,W34:B12,B13,B14,B15
//    // - W:W1,2,3,4:B21,22,23,24
//    // - W:WK31,W32:B12,B13,BK14
//    private static readonly Regex PdnRegex =
//    new(@"^[BW]:(W[K]?\d+(,W[K]?\d+)*)?:B[K]?\d+(,B[K]?\d+)*$", RegexOptions.Compiled);

//    public static bool IsValid(string pdn)
//    {
//        if (string.IsNullOrWhiteSpace(pdn))
//            return false;

//        return PdnRegex.IsMatch(pdn.Trim());
//    }
//}

using System.Text.RegularExpressions;

namespace CheckersApi.Validation;

public static class PdnValidator
{
    private static readonly Regex PdnRegex =
        new(@"^[BW]:W[K]?\d+(,[K]?\d+)*:B[K]?\d+(,[K]?\d+)*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static bool IsValid(string pdn)
    {
        if (string.IsNullOrWhiteSpace(pdn))
            return false;

        var norm = PdnNormalizer.Normalize(pdn);
        return PdnRegex.IsMatch(norm);
    }
}