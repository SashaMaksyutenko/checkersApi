using System.Runtime.InteropServices;

namespace CheckersApi.Engine
{
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

                _initialized = true;
            }
        }

        private static void LoadAbsolute(string fullPath)
        {
            if (!File.Exists(fullPath))
                throw new DllNotFoundException($"DLL not found: {fullPath}");

            var handle = NativeLibrary.Load(fullPath);
            if (handle == IntPtr.Zero)
                throw new DllNotFoundException($"Failed to load: {fullPath}");
        }
    }
}
