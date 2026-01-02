using System.Diagnostics;
using System.Text;

namespace CheckersApi.Engine;

public sealed class ChinookProcess : IDisposable
{
    private readonly Process _process;
    private readonly StreamWriter _stdin;
    private readonly StreamReader _stdout;

    public ChinookProcess(string exePath)
    {
        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        _process.Start();

        _stdin = _process.StandardInput;
        _stdout = _process.StandardOutput;

        warmup();
    }

    private void warmup()
    {
        // chinook очікує команди в checkerboard протоколі
        send("name");
        readUntilReady();
    }

    public void SetPosition(string pdn)
    {
        send($"setboard {pdn}");
    }

    public SearchResult Search(int depth, int timeMs)
    {
        send($"go depth {depth} time {timeMs}");
        return parseSearchOutput();
    }

    public string ProbeTablebase(string pdn)
    {
        send($"probe {pdn}");
        var line = _stdout.ReadLine() ?? "";

        // chinook повертає щось типу "move 22-18" або "draw"
        if (line.StartsWith("move "))
            return line[5..].Trim();

        return string.Empty;
    }

    private void send(string cmd)
    {
        _stdin.WriteLine(cmd);
        _stdin.Flush();
    }

    private void readUntilReady()
    {
        while (true)
        {
            var line = _stdout.ReadLine();
            if (line == null || line.Contains("ready", StringComparison.OrdinalIgnoreCase))
                break;
        }
    }

    private SearchResult parseSearchOutput()
    {
        var result = new SearchResult();
        var pvMoves = new List<string>();

        while (true)
        {
            var line = _stdout.ReadLine();
            if (line == null)
                break;

            // приклад: "info depth 12 score 0 nodes 153201 pv 22-18 5-9 18x11"
            if (line.StartsWith("info "))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                for (int i = 1; i < parts.Length; i++)
                {
                    if (parts[i] == "depth" && i + 1 < parts.Length)
                    {
                        if (int.TryParse(parts[i + 1], out var d))
                            result.Depth = d;
                    }

                    if (parts[i] == "nodes" && i + 1 < parts.Length)
                    {
                        if (long.TryParse(parts[i + 1], out var n))
                            result.Nodes = n;
                    }

                    if (parts[i] == "score" && i + 1 < parts.Length)
                    {
                        if (int.TryParse(parts[i + 1], out var s))
                            result.Score = s;
                    }

                    if (parts[i] == "pv")
                    {
                        // всі ходи після "pv" - це principal variation
                        for (int j = i + 1; j < parts.Length; j++)
                            pvMoves.Add(parts[j]);
                        break;
                    }
                }
            }

            // приклад: "bestmove 22-18"
            if (line.StartsWith("bestmove "))
            {
                result.BestMove = line[9..].Trim();
                result.Pv = pvMoves.ToArray();
                break;
            }
        }

        return result;
    }

    public void Dispose()
    {
        try
        {
            send("quit");
            if (!_process.WaitForExit(1000))
                _process.Kill();
        }
        catch { }

        _process.Dispose();
    }
}

public class SearchResult
{
    public string BestMove { get; set; } = string.Empty;
    public string[] Pv { get; set; } = [];
    public int Score { get; set; }
    public int Depth { get; set; }
    public long Nodes { get; set; }
}

