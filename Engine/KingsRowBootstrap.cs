using CheckersApi.Engine;
using System.Runtime.InteropServices;

public static class KingsRowBootstrap
{
    private static bool _initialized;
    private static readonly object _lock = new();

    public static void Initialize(string dbPath, bool useInit)
    {
        if (_initialized) return;

        lock (_lock)
        {
            if (_initialized) return;

            var baseDir = AppContext.BaseDirectory;

            LoadAbsolute(Path.Combine(baseDir, "egdb64.dll"));
            LoadAbsolute(Path.Combine(baseDir, "Kingsrow64.dll"));

            if (useInit && !string.IsNullOrWhiteSpace(dbPath))
            {
                // Convert relative path to absolute
                var fullDbPath = Path.IsPathRooted(dbPath)
                    ? dbPath
                    : Path.Combine(baseDir, dbPath);

                var cmd = $"init {fullDbPath}";
                Console.WriteLine($"[KingsRowBootstrap] Initializing with: {cmd}");
                var rc = NativeKingsRow.enginecommand(cmd);
                if (rc != 0)
                    throw new InvalidOperationException($"KingsRow init rc={rc} path={fullDbPath}");

                Console.WriteLine($"[KingsRowBootstrap] Database initialized successfully");
            }

            _initialized = true;
        }
    }

    private static void LoadAbsolute(string fullPath)
    {
        if (!File.Exists(fullPath))
        {
            Console.WriteLine($"[KingsRowBootstrap] DLL not found: {fullPath}");
            return;
        }

        var handle = NativeLibrary.Load(fullPath);
        if (handle == IntPtr.Zero)
            throw new DllNotFoundException($"Failed to load: {fullPath}");
    }
}