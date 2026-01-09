using GuestFlow.Domain.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [ApiController]
    [Route("api/dev")]
    public class DevController : ControllerBase
    {
        private readonly IDataProtection _dataProtection;
        private readonly IWebHostEnvironment _env;

        public DevController(IDataProtection dataProtection, IWebHostEnvironment env)
        {
            _dataProtection = dataProtection;
            _env = env;
        }

        public class ProtectRequest
        {
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("protect-password")]
        public IActionResult ProtectPassword([FromBody] ProtectRequest request)
        {
            // Only allow in Development environment
            if (!_env.IsDevelopment())
                return NotFound();

            if (string.IsNullOrEmpty(request?.Password))
                return BadRequest(new { message = "Password is required." });

            var protectedValue = _dataProtection.Protect(request.Password);
            return Ok(new { protectedPassword = protectedValue });
        }
    }
}

