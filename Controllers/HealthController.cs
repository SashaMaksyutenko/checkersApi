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
        // пробуємо отримати worker pool, але якщо немає - повертаємо 1
        // для kingsrow dll воркери не використовуються, тому fallback
        var chinookPool = HttpContext.RequestServices.GetService<ChinookWorkerPool>();
        var workers = chinookPool?.Count ?? 1;

        return Ok(new { ok = true, workers });
    }
}