//using CheckersApi.Contracts;
//using CheckersApi.Engine;
//using CheckersApi.Validation;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;

//namespace CheckersApi.Controllers;

//[ApiController]
//[Route("v1/move")]
//public class MoveController : ControllerBase
//{
//    private readonly IEngineAdapter _engine;

//    public MoveController(IEngineAdapter engine)
//    {
//        _engine = engine;
//    }

//    [HttpPost("suggest")]
//    public IActionResult Suggest([FromBody] SuggestRequest request)
//    {

//        if (request?.State?.Position is null)
//            return BadRequest(new { error = "State.Position is required" });

//        //2️⃣ PDN валідація
//        if (!PdnValidator.IsValid(request.State.Position))
//            return UnprocessableEntity(new { error = "Invalid PDN format" });

//        //3️⃣ hard timeout(ЗА ТЗ)
//        var hardMs = request.Limits?.HardTimeMs ?? 1200;

//        using var cts = CancellationTokenSource
//            .CreateLinkedTokenSource(HttpContext.RequestAborted);

//        cts.CancelAfter(hardMs);

//        try
//        {
//            //делегуємо в engine
//           var response = _engine.Suggest(request, cts.Token);
//            return Ok(response);
//        }
//        catch (OperationCanceledException)
//        {
//            return StatusCode(504, new { error = "Engine timeout" });
//        }
//    }

//    [HttpPost("validate")]
//    public IActionResult Validate([FromBody] ValidateMoveRequest request)
//    {
//        if (request is null)
//            return BadRequest();

//        bool legal = MoveValidator.IsLegal(
//            request.Position,
//            request.Move
//        );

//        return Ok(new { legal });
//    }
//}


using CheckersApi.Contracts;
using CheckersApi.Engine;
using CheckersApi.Validation;
using CheckersApi.Logging;
using Microsoft.AspNetCore.Mvc;

namespace CheckersApi.Controllers;

[ApiController]
[Route("v1/move")]
public class MoveController : ControllerBase
{
    private readonly IEngineAdapter _engine;
    private readonly ILogger<MoveController> _logger;

    public MoveController(IEngineAdapter engine, ILogger<MoveController> logger)
    {
        _engine = engine;
        _logger = logger;
    }

    [HttpPost("suggest")]
    public IActionResult Suggest([FromBody] SuggestRequest request)
    {
        if (request?.State?.Position is null)
            return BadRequest(new { error = "State.Position is required" });

        // генеруємо requestId для логів
        var requestId = Guid.NewGuid().ToString("N")[..8];

        // Normalize then validate per TЗ
        var pdn = PdnNormalizer.Normalize(request.State.Position);
        if (!PdnValidator.IsValid(pdn))
            return UnprocessableEntity(new { error = "Invalid PDN format" });

        // Hard timeout (controller-level)
        var hardMs = request.Limits?.HardTimeMs ?? 1200;
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
        cts.CancelAfter(hardMs);

        try
        {
            // Ensure normalized PDN is used for cache keys and adapter
            request.State.Position = pdn;

            var response = _engine.Suggest(request, cts.Token);

            // логуємо результат в json
            RequestLogger.LogRequest(
                _logger,
                requestId,
                response.Info?.TimeMs ?? 0,
                response.Depth,
                response.Nodes,
                response.Info?.TablebaseHit ?? false
            );

            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(504, new { error = "Timeout" });
        }
        catch (ArgumentException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("validate")]
    public IActionResult Validate([FromBody] ValidateMoveRequest request)
    {
        if (request is null)
            return BadRequest(new { error = "Invalid request" });

        var pdn = PdnNormalizer.Normalize(request.Position ?? "");
        if (!PdnValidator.IsValid(pdn))
            return UnprocessableEntity(new { error = "Invalid PDN format" });

        var legal = MoveValidator.IsLegalFormat(request.Move ?? "");
        return Ok(new { legal });
    }
}