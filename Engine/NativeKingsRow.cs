using System.Runtime.InteropServices;
using System.Text;

namespace CheckersApi.Engine
{
    internal static class NativeKingsRow
    {
        [DllImport("Kingsrow64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int getmove(
            string pdnPosition,
            StringBuilder moveBuffer,
            int bufferSize);

        [DllImport("Kingsrow64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int get_best_moves(
            string pdnPosition,
            int maxDepth,
            StringBuilder moveBuffer,
            int bufferSize);

        [DllImport("Kingsrow64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int staticevaluation(string pdnPosition);

        [DllImport("Kingsrow64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int enginecommand(string command);
    }
}
