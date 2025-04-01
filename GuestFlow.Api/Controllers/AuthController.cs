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
        // Burada kullanacağım değişkenleri tanımlıyorum.
        // _personnelService: Personel işlemleri için servisi kullanıyorum.
        // _logger: Hataları veya bilgileri loglamak için kullanıyorum.
        private readonly IPersonnelService _personnelService;
        private readonly ILogger<AuthController> _logger;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public AuthController(IPersonnelService personnelService, ILogger<AuthController> logger)
        {
            _personnelService = personnelService;
            _logger = logger;
        }

        // Bu metodumla yeni bir kullanıcı kaydı yapıyorum.
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            // Gelen isteğin doğruluğunu kontrol ediyorum. Eğer model geçersizse, hata döndürüyorum.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
                // TODO: İlerde action filter ile yapılacak
            }

            // Gelen isteği bir DTO'ya çeviriyorum ki serviste kullanabileyim.
            var addPersonnelDto = new AddPersonnelDto
            {
                Email = request.Email,
                FullName = request.FullName,
                Password = request.Password
            };

            // Kullanıcıyı kaydetmek için servisi çağırıyorum.
            var result = await _personnelService.AddPersonnel(addPersonnelDto);

            if (result.IsSuccess)
            {
                // Eğer kayıt başarılıysa, başarı mesajını JSON formatında döndürüyorum.
                return Ok(new { message = "Kayıt başarıyla tamamlandı." });
            }
            else
            {
                // Eğer kayıt başarısızsa, hata mesajını JSON formatında döndürüyorum.
                return BadRequest(new { message = result.Message });
            }
        }

        // Bu metodumla kullanıcı girişi yapıyorum ve bir JWT token üretiyorum.
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Model geçersizse, bunu logluyorum ve hata döndürüyorum.
                    _logger.LogWarning($"Geçersiz model durumu: {ModelState}");
                    return BadRequest(ModelState);
                }

                // Giriş denemesini logluyorum.
                _logger.LogInformation($"Giriş denemesi: {request.Email}");
                // Servisten giriş işlemini yapıyorum.
                var result = await _personnelService.Login(new LoginPersonnelDto
                {
                    Email = request.Email,
                    Password = request.Password
                });

                if (!result.IsSuccess)
                {
                    // Eğer giriş başarısızsa, bunu logluyorum ve hata mesajı döndürüyorum.
                    _logger.LogWarning($"Giriş başarısız: {result.Message}");
                    return BadRequest(new { message = result.Message });
                }

                // Giriş başarılıysa, kullanıcı bilgilerini alıyorum.
                var user = result.Data;
                _logger.LogInformation($"Kullanıcı bilgileri alındı: Email: {user.Email}, UserType: {user.UserType}");

                // JWT token oluşturmak için yapılandırma ayarlarını alıyorum.
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

                // Token'ın oluşturulduğunu logluyorum.
                _logger.LogInformation($"Token oluşturuldu: Email: {user.Email}, Role: {user.UserType}");
                // Başarı mesajı ve token'ı JSON formatında döndürüyorum.
                return Ok(new LoginResponse
                {
                    Message = "Giriş başarıyla tamamlandı",
                    Token = token
                });
            }
            catch (Exception ex)
            {
                // Eğer bir hata çıkarsa, bunu logluyorum ve 500 hata koduyla hata mesajı döndürüyorum.
                _logger.LogError(ex, $"Giriş sırasında hata çıktı: {request.Email}. InnerException: {ex.InnerException?.Message}");
                return StatusCode(500, new { message = "Giriş sırasında bir hata oluştu." });
            }
        }

        // Bu metodumla giriş yapmış kullanıcının bilgilerini kontrol ediyorum.
        [HttpGet("me")]
        [Authorize] // Bu endpoint'e sadece geçerli bir token ile erişilebilir.
        public IActionResult GetMyUser()
        {
            // Şu an için sadece bir kontrol endpoint'i, bu yüzden boş bir başarı yanıtı döndürüyorum.
            return Ok();
        }
    }
}