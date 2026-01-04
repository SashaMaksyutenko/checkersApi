using System.Diagnostics;
using System.Text;
using CheckersApi.Contracts;
using CheckersApi.Validation;

namespace CheckersApi.Engine
{
    public class KingsRowAdapter : IEngineAdapter
    {
        private readonly string _dbPath;

        public KingsRowAdapter(string dbPath)
        {
            _dbPath = dbPath;
            KingsRowBootstrap.Initialize(_dbPath, useInit: false);
        }

        public SuggestResponse Suggest(SuggestRequest request, CancellationToken ct)
        {
            if (request?.State?.Position is null)
                throw new ArgumentException("State.Position is required");

            var pdn = request.State.Position;
            var (depth, _) = SearchLimitsResolver.Resolve(request.Level ?? "weak");
            if (request.Limits?.MaxDepth != null)
                depth = request.Limits.MaxDepth.Value;

            var sw = Stopwatch.StartNew(); // просто створюємо та запускаємо

            var sb = new StringBuilder(8192);
            int rc = NativeKingsRow.get_best_moves(pdn, depth, sb, sb.Capacity);

            if (rc != 0 || sb.Length == 0)
                throw new InvalidOperationException($"KingsRow failed. rc={rc}, pos={pdn}");

            var move = sb.ToString().Trim();
            if (!MoveValidator.IsReasonable(move))
                throw new InvalidOperationException($"Engine returned invalid move: {move}");

            sw.Stop();

            return new SuggestResponse
            {
                Engine = "kingsrow",
                BestMove = move,
                PositionKey = PdnNormalizer.ToPositionKey(pdn),
                Depth = depth,
                Nodes = 0,
                Info = new SuggestInfo
                {
                    TablebaseHit = false,
                    TimeMs = (int)sw.ElapsedMilliseconds,
                    Evaluation = NativeKingsRow.staticevaluation(pdn)
                }
            };
        }
    }
}
