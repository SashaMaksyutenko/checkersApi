namespace CheckersApi.Contracts;

public class SuggestRequest
{
    public string GameId { get; set; } = "checkers-8x8";
    public StateDto State { get; set; } = default!;
    public string Level { get; set; } = "weak";
    public LimitsDto? Limits { get; set; } = null!;
}
