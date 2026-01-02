namespace CheckersApi.Contracts;

public class LimitsDto
{
    public int? MaxDepth { get; set; }
    public int? SoftTimeMs { get; set; }
    public int? HardTimeMs { get; set; }
}
