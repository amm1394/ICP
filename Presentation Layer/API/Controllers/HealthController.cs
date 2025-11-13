using Microsoft.AspNetCore.Mvc;

namespace Presentation.Icp.API.Controllers
{
    /// <summary>
    /// Provides a simple health check endpoint for the API.
    /// </summary>
    /// <remarks>
    /// This controller exposes a lightweight GET endpoint intended for liveness/readiness probes
    /// (e.g., container orchestrators, load balancers, or uptime monitors).
    /// The response payload contains:
    /// - Status: A human-readable status string (localized in Persian for end users).
    /// - Timestamp: The current server time in UTC.
    /// - Version: The application version string.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Returns a simple health status payload indicating the API is responsive.
        /// </summary>
        /// <returns>
        /// 200 OK with a JSON object containing Status (Persian), Timestamp (UTC), and Version.
        /// </returns>
        /// <response code="200">The API is healthy and responsive.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return Ok(new
            {
                Status = "سالم",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0"
            });
        }
    }
}