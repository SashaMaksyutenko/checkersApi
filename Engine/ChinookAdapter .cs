using System.Diagnostics;
using CheckersApi.Contracts;
using CheckersApi.Validation;

namespace CheckersApi.Engine
{
    public class ChinookAdapter : IEngineAdapter
    {
        private readonly ChinookWorkerPool _pool;

        public ChinookAdapter(ChinookWorkerPool pool)
        {
            _pool = pool;
        }

        public SuggestResponse Suggest(SuggestRequest request, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            var pdn = PdnNormalizer.Normalize(request.State.Position);

            if (!PdnValidator.IsValid(pdn))
                throw new ArgumentException("Invalid PDN format");

            var pieceCount = PdnUtils.CountPieces(pdn);
            var worker = _pool.Next();

            // якщо <= 8 фігур, пробуємо tablebase
            if (pieceCount <= 8)
            {
                var tbMove = worker.ProbeTablebaseAsync(pdn, ct).Result;
                if (!string.IsNullOrEmpty(tbMove) && MoveValidator.IsReasonable(tbMove))
                {
                    sw.Stop();
                    return new SuggestResponse
                    {
                        Engine = "chinook",
                        BestMove = tbMove,
                        Pv = new[] { tbMove },
                        PositionKey = PdnNormalizer.ToPositionKey(pdn),
                        Depth = 0,
                        Nodes = 0,
                        Info = new SuggestInfo
                        {
                            TablebaseHit = true,
                            TimeMs = (int)sw.ElapsedMilliseconds
                        }
                    };
                }
            }

            var (depth, softMs) = SearchLimitsResolver.Resolve(request.Level ?? "weak");

            // якщо юзер передав свої ліміти - використовуємо їх
            if (request.Limits?.MaxDepth != null)
                depth = request.Limits.MaxDepth.Value;

            if (request.Limits?.SoftTimeMs != null)
                softMs = request.Limits.SoftTimeMs.Value;

            var result = worker.SearchAsync(pdn, depth, softMs, ct).Result;
            sw.Stop();

            // перевіряємо що хід адекватний
            if (!MoveValidator.IsReasonable(result.BestMove))
                throw new InvalidOperationException($"Engine returned invalid move: {result.BestMove}");

            return new SuggestResponse
            {
                Engine = "chinook",
                BestMove = result.BestMove,
                Pv = result.Pv,
                ScoreOrWdl = result.Score,
                PositionKey = PdnNormalizer.ToPositionKey(pdn),
                Depth = result.Depth,
                Nodes = result.Nodes,
                Info = new SuggestInfo
                {
                    TablebaseHit = false,
                    TimeMs = (int)sw.ElapsedMilliseconds
                }
            };
        }
    }
}
