using System.Text.Json;

namespace CheckersApi.Logging;

public static class RequestLogger
{
    // логуємо json для кожного запиту
    public static void LogRequest(
        ILogger logger,
        string requestId,
        int timeMs,
        int depth,
        long nodes,
        bool tablebaseHit)
    {
        var json = JsonSerializer.Serialize(new
        {
            requestId,
            timeMs,
            depth,
            nodes,
            tablebaseHit
        });

        logger.LogInformation(json);
    }
}
