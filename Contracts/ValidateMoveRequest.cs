namespace CheckersApi.Contracts;

public class ValidateMoveRequest
{
    public string Position { get; set; } = default!;
    public string Move { get; set; } = default!;
}
