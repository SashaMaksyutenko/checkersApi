using System.Diagnostics;
using System.Text;
using CheckersApi.Contracts;
using CheckersApi.Validation;

namespace CheckersApi.Engine
{
    public class KingsRowAdapter : IEngineAdapter
    {
        private readonly string _dbPath;
        private readonly bool _useInit;

        public KingsRowAdapter(string dbPath, bool useInit = false)
        {
            _dbPath = dbPath;
            _useInit = useInit;

            KingsRowBootstrap.Initialize(_dbPath, _useInit); // UseInit має бути false
        }

        public SuggestResponse Suggest(SuggestRequest request, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            var pdn = request.State.Position;

            // підраховуємо скільки фігур на дошці
            var pieceCount = PdnUtils.CountPieces(pdn);

            // TEMPORARILY DISABLED: tablebase lookup causes crashes without proper init
            // TODO: fix database initialization
            // якщо <= 8 фігур, пробуємо tablebase
            //if (pieceCount <= 8)
            //{
            //    var tbMove = tryTablebase(pdn);
            //    if (!string.IsNullOrEmpty(tbMove) && MoveValidator.IsReasonable(tbMove))
            //    {
            //        sw.Stop();
            //        return new SuggestResponse
            //        {
            //            Engine = "kingsrow",
            //            BestMove = tbMove,
            //            PositionKey = PdnNormalizer.ToPositionKey(pdn),
            //            Depth = 0,
            //            Nodes = 0,
            //            Info = new SuggestInfo
            //            {
            //                TablebaseHit = true,
            //                TimeMs = (int)sw.ElapsedMilliseconds,
            //                Evaluation = 0
            //            }
            //        };
            //    }
            //}

            // визначаємо depth і time based on level
            var (depth, timeMs) = SearchLimitsResolver.Resolve(request.Level ?? "weak");

            // якщо юзер передав свої ліміти - використовуємо їх
            if (request.Limits?.MaxDepth != null)
                depth = request.Limits.MaxDepth.Value;

            var sb = new StringBuilder(8192);
            int rc;

            try
            {
                rc = NativeKingsRow.get_best_moves(
                    pdn,
                    depth,
                    sb,
                    sb.Capacity
                );
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"get_best_moves crashed: {ex.Message}", ex);
            }

            if (rc != 0 || sb.Length == 0)
                throw new InvalidOperationException($"KingsRow failed. rc={rc}, pos={pdn}");

            sw.Stop();
            var move = sb.ToString().Trim();

            // перевіряємо що хід виглядає адекватно
            if (!MoveValidator.IsReasonable(move))
            {
                throw new InvalidOperationException($"Engine returned invalid move: {move}");
            }

            return new SuggestResponse
            {
                Engine = "kingsrow",
                BestMove = move,
                PositionKey = PdnNormalizer.ToPositionKey(pdn),
                Depth = depth,
                Nodes = 0, // kingsrow dll не повертає nodes, залишаємо 0
                Info = new SuggestInfo
                {
                    TablebaseHit = false,
                    TimeMs = (int)sw.ElapsedMilliseconds,
                    Evaluation = NativeKingsRow.staticevaluation(pdn)
                }
            };
        }

        // пробуємо витягнути хід з tablebase
        private string tryTablebase(string pdn)
        {
            try
            {
                var sb = new StringBuilder(256);
                // getmove - це швидкий tablebase lookup
                var rc = NativeKingsRow.getmove(pdn, sb, sb.Capacity);

                if (rc == 0 && sb.Length > 0)
                {
                    return sb.ToString().Trim();
                }
            }
            catch
            {
                // якщо tablebase не спрацював - нічого страшного
            }

            return string.Empty;
        }
    }
}