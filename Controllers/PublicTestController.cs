using Microsoft.AspNetCore.Mvc;

namespace SmartCard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublicTestController : ControllerBase
    {
        [HttpGet("ping")]
        public ActionResult<string> Ping()
        {
            return Ok("Pong - API is working!");
        }

        [HttpGet("health")]
        public ActionResult<string> Health()
        {
            return Ok("API is healthy and running");
        }
    }
}