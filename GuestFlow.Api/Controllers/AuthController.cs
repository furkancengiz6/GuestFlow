using GuestFlow.Api.Jwt;
using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.Personnel;
using GuestFlow.Application.Operations.Personnel.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IPersonnelService _personnelService;
        private readonly ILogger<AuthController> _logger; // Logger bağımlılığı eklendi

        public AuthController(IPersonnelService personnelService, ILogger<AuthController> logger)
        {
            _personnelService = personnelService;
            _logger = logger; // Logger bağımlılığı constructor'da tanımlandı
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
                // TODO: İlerde action filter ile yapılacak
            }

            var addPersonnelDto = new AddPersonnelDto
            {
                Email = request.Email,
                FullName = request.FullName,
                Password = request.Password
            };

            var result = await _personnelService.AddPersonnel(addPersonnelDto);

            if (result.IsSuccess)
            {
                return Ok(new { message = "Kayıt başarıyla tamamlandı." });
            }
            else
            {
                return BadRequest(new { message = result.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Geçersiz model durumu: {ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Giriş denemesi: {Email}", request.Email);
                var result = await _personnelService.Login(new LoginPersonnelDto
                {
                    Email = request.Email,
                    Password = request.Password
                });

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Giriş başarısız: {Message}", result.Message);
                    return BadRequest(new { message = result.Message });
                }

                var user = result.Data;
                _logger.LogInformation("Kullanıcı bilgileri alındı: Email: {Email}, UserType: {UserType}", user.Email, user.UserType);

                var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var token = JwtHelper.GenerateJwtToken(new JwtDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    UserType = user.UserType,
                    SecretKey = configuration["Jwt:SecretKey"]!,
                    Issuer = configuration["Jwt:Issuer"]!,
                    Audience = configuration["Jwt:Audience"]!,
                    ExpireMinutes = int.Parse(configuration["Jwt:ExpireMinutes"]!)
                });

                _logger.LogInformation("Token oluşturuldu: Email: {Email}, Role: {Role}", user.Email, user.UserType);
                return Ok(new LoginResponse
                {
                    Message = "Giriş başarıyla tamamlandı",
                    Token = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Giriş sırasında hata oluştu: {Email}", request.Email);
                return StatusCode(500, new { message = "Giriş sırasında bir hata oluştu." });
            }
        }

        [HttpGet("me")]
        [Authorize] // Token yok cevap yok
        public IActionResult GetMyUser()
        {
            return Ok();
        }
    }
}