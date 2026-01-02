//using System.Text.RegularExpressions;

//namespace CheckersApi.Validation;

//public static class MoveValidator
//{
//    private static readonly Regex MoveRegex =
//        new(@"^\d{2}(-\d{2})+$", RegexOptions.Compiled);

//    public static bool IsLegal(string position, string move)
//    {
//        if (string.IsNullOrWhiteSpace(position) ||
//            string.IsNullOrWhiteSpace(move))
//            return false;

//        return MoveRegex.IsMatch(move);
//    }
//}
using System.Text.RegularExpressions;

namespace CheckersApi.Validation;

public static class MoveValidator
{
    // Accepts both simple moves and capture chains with mixed '-' and 'x':
    // Examples: 22-18, 22-18x11-7, 12x19-26
    private static readonly Regex MoveRegex =
        new(@"^\d{2}([\-x]\d{2})+$", RegexOptions.Compiled);

    public static bool IsLegalFormat(string move)
    {
        return !string.IsNullOrWhiteSpace(move) && MoveRegex.IsMatch(move);
    }

    // перевіряємо чи хід виглядає нормально (не пустий і має правильний формат)
    public static bool IsReasonable(string move)
    {
        if (string.IsNullOrWhiteSpace(move))
            return false;

        // хід має бути типу "22-18" або "22-18x11"
        if (!IsLegalFormat(move))
            return false;

        // перевіряємо що числа в межах 1-32 (стандартна шашкова дошка)
        var parts = move.Split(new[] { '-', 'x' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            if (!int.TryParse(part, out var square) || square < 1 || square > 32)
                return false;
        }

        return true;
    }
}