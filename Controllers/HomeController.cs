using Microsoft.AspNetCore.Mvc;

namespace CheckersApi.Controllers
{
    [ApiController]
    [Route("/")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "Checkers API is running!" });
        }
    }
}