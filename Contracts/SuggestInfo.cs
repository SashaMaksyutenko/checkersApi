namespace CheckersApi.Contracts;

public class SuggestInfo
{
    public bool TablebaseHit { get; set; }
    public int TimeMs { get; set; }
    public int Evaluation { get; set; }
}
