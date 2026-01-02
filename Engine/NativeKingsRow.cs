//using System.Runtime.InteropServices;
//using System.Text;

//namespace CheckersApi.Engine
//{
//    public static class NativeKingsRow
//    {
//        [DllImport("Kingsrow64.dll", CallingConvention = CallingConvention.Cdecl)]
//        public static extern int get_best_moves(string pdnPosition, int maxDepth, StringBuilder moveBuffer, int bufferSize);

//        [DllImport("Kingsrow64.dll", CallingConvention = CallingConvention.Cdecl)]
//        public static extern int getmove(string pdnPosition, StringBuilder moveBuffer, int bufferSize);

//        [DllImport("Kingsrow64.dll", CallingConvention = CallingConvention.Cdecl)]
//        public static extern int staticevaluation(string pdnPosition);

//        // enginecommand приймає один рядок (наприклад "init C:\\kr_english_wld")
//        [DllImport("Kingsrow64.dll", CallingConvention = CallingConvention.Cdecl)]
//        public static extern int enginecommand(string command);
//    }
//}
using System.Runtime.InteropServices;
using System.Text;

namespace CheckersApi.Engine
{
    internal static class NativeKingsRow
    {
        // Kingsrow64.dll експортує функції з cdecl; рядки — ANSI
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

        // Якщо enginecommand потрібен, залишити, але НЕ викликати при UseInit=false
        [DllImport("Kingsrow64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int enginecommand(string command);
    }
}