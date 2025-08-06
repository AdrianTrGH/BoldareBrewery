using Asp.Versioning;
using BoldareBrewery.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoldareBrewery.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        /// <summary>
        public AuthController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost("token")]
        public IActionResult GetToken([FromBody] LoginRequest? request = null)
        {

            var token = _jwtService.GenerateToken();

            return Ok(new
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                TokenType = "Bearer",
                Message = "This is a demo token for testing purposes"
            });
        }      
    }

    public class LoginRequest
    {
        public string Username { get; set; } = "demo";
        public string Password { get; set; } = "demo";
    }
}
