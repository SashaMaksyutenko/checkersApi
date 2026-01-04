using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using CheckersApi.Engine;

[ApiController]
[Route("/healthz")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { ok = true, workers=1 });
    }
}