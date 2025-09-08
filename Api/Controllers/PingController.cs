using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// Ping Controller
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PingController : ControllerBase
    {
        /// <summary>
        /// pinging
        /// </summary>
        [HttpGet]
        public IActionResult Ping()
        {
            return Ok("Pong!");
        }

        /// <summary>
        /// the newest api version - shuld be on environment-variable or in 'appsettings.json'
        /// </summary>
        /// <returns>
        /// the current api version
        /// </returns>
        [HttpGet]
        public IActionResult VersionApi()
        {
            return Ok("1.0.1");
        }
    }
}
