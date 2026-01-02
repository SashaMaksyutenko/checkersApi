namespace CheckersApi.Contracts;

public class SuggestResponse
{
    public string Engine { get; set; } = "chinook";
    public string BestMove { get; set; } = default!;
    public string[] Pv { get; set; } = [];
    public int ScoreOrWdl { get; set; }
    public int Depth { get; set; }
    public long Nodes { get; set; }
    public string PositionKey { get; set; } = default!;
    public SuggestInfo Info { get; set; } = new();
}

