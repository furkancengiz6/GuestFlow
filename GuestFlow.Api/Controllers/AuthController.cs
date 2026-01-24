using GuestFlow.Api.Jwt;
using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.Auth;
using GuestFlow.Application.Operations.Password;
using GuestFlow.Application.Operations.Personnel;
using GuestFlow.Application.Operations.Personnel.Dtos;
using GuestFlow.Domain.Entities.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GuestFlow.Api.Filters;

namespace GuestFlow.Api.Controllers
{


    /// <summary>
    /// Kimlik doğrulama ve yetkilendirme işlemleri için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Tags("Kimlik Doğrulama")]
    [ValidateModel]
    public class AuthController : ControllerBase
    {
        // Burada kullanacağım değişkenleri tanımlıyorum.
        // _personnelService: Personel işlemleri için servisi kullanıyorum.
        // _refreshTokenService: Refresh token işlemleri için servisi kullanıyorum.
        // _passwordService: Şifre işlemleri için servisi kullanıyorum.
        // _logger: Hataları veya bilgileri loglamak için kullanıyorum.
        private readonly IPersonnelService _personnelService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IPasswordService _passwordService;
        private readonly ITwoFactorService _twoFactorService;
        private readonly IBruteForceProtectionService _bruteForceProtection;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public AuthController(
            IPersonnelService personnelService,
            IRefreshTokenService refreshTokenService,
            IPasswordService passwordService,
            ITwoFactorService twoFactorService,
            IBruteForceProtectionService bruteForceProtection,
            ILogger<AuthController> logger,
            IConfiguration configuration)
        {
            _personnelService = personnelService;
            _refreshTokenService = refreshTokenService;
            _passwordService = passwordService;
            _twoFactorService = twoFactorService;
            _bruteForceProtection = bruteForceProtection;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Yeni bir kullanıcı kaydı oluşturur
        /// </summary>
        /// <param name="request">Kayıt bilgileri (Email, FullName, Password)</param>
        /// <returns>Kayıt sonucu</returns>
        /// <response code="200">Kayıt başarıyla tamamlandı</response>
        /// <response code="400">Geçersiz istek verisi veya kayıt hatası</response>
        /// <example>
        /// <code>
        /// POST /api/v1/auth/register
        /// {
        ///   "fullName": "Ahmet Yılmaz",
        ///   "email": "ahmet.yilmaz@example.com",
        ///   "password": "SecurePass123!"
        /// }
        /// </code>
        /// </example>
        [HttpPost("register")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
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

        /// <summary>
        /// Kullanıcı girişi yapar ve JWT access token ile refresh token üretir
        /// </summary>
        /// <param name="request">Giriş bilgileri (Email, Password)</param>
        /// <returns>Access token ve refresh token</returns>
        /// <response code="200">Giriş başarılı, token'lar döndürüldü</response>
        /// <response code="400">Geçersiz istek verisi veya giriş başarısız</response>
        /// <response code="500">Sunucu hatası</response>
        /// <example>
        /// <code>
        /// POST /api/v1/auth/login
        /// {
        ///   "email": "admin@example.com",
        ///   "password": "Admin123!"
        /// }
        /// 
        /// Response:
        /// {
        ///   "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        ///   "refreshToken": "abc123def456...",
        ///   "expiresIn": 3600
        /// }
        /// </code>
        /// </example>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                // Brute-force protection: Check if login is allowed
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var isLoginAllowed = await _bruteForceProtection.IsLoginAllowedAsync(request.Email, ipAddress);
                
                if (!isLoginAllowed)
                {
                    var remainingTime = await _bruteForceProtection.GetRemainingLockoutTimeAsync(request.Email, ipAddress);
                    _logger.LogWarning($"Login blocked for {request.Email} from IP {ipAddress} - too many failed attempts. Remaining lockout: {remainingTime} seconds");
                    
                    // Record failed attempt
                    await _bruteForceProtection.RecordLoginAttemptAsync(request.Email, ipAddress, false, "AccountLocked", null);
                    
                    return BadRequest(new { 
                        message = $"Çok fazla başarısız giriş denemesi. Lütfen {remainingTime / 60} dakika sonra tekrar deneyin.",
                        lockoutRemainingSeconds = remainingTime
                    });
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
                    // Record failed login attempt
                    await _bruteForceProtection.RecordLoginAttemptAsync(request.Email, ipAddress, false, result.Message, null);
                    
                    // Eğer giriş başarısızsa, bunu logluyorum ve hata mesajı döndürüyorum.
                    _logger.LogWarning($"Giriş başarısız: {result.Message}");
                    
                    var failedCount = await _bruteForceProtection.GetFailedAttemptCountAsync(request.Email, ipAddress);
                    var maxFailedAttempts = int.TryParse(_configuration["Security:BruteForce:MaxFailedAttempts"], out var maxAttempts) ? maxAttempts : 5;
                    var remainingAttempts = maxFailedAttempts - failedCount;
                    
                    return BadRequest(new { 
                        message = result.Message,
                        remainingAttempts = remainingAttempts > 0 ? remainingAttempts : 0
                    });
                }

                // Giriş başarılıysa, kullanıcı bilgilerini alıyorum.
                var user = result.Data;
                if (user is null)
                {
                    _logger.LogError("Giriş başarılı görünüyor ancak kullanıcı bilgisi null döndü.");
                    return StatusCode(500, new { message = "Giriş sırasında bir hata oluştu." });
                }
                _logger.LogInformation($"Kullanıcı bilgileri alındı: Email: {user.Email}, UserType: {user.UserType}");

                // Check if 2FA is enabled for this user
                var is2FAEnabled = await _twoFactorService.IsEnabledAsync(user.Id);
                if (is2FAEnabled)
                {
                    // Verify 2FA code or recovery code
                    bool is2FAValid = false;
                    if (!string.IsNullOrEmpty(request.TwoFactorCode))
                    {
                        is2FAValid = await _twoFactorService.VerifyCodeAsync(user.Id, request.TwoFactorCode);
                    }
                    else if (!string.IsNullOrEmpty(request.RecoveryCode))
                    {
                        is2FAValid = await _twoFactorService.VerifyRecoveryCodeAsync(user.Id, request.RecoveryCode);
                    }

                    if (!is2FAValid)
                    {
                        _logger.LogWarning($"2FA verification failed for user {user.Email}");
                        return BadRequest(new { 
                            message = "2FA doğrulaması başarısız. Lütfen 2FA kodunuzu veya recovery kodunuzu girin.",
                            requiresTwoFactor = true 
                        });
                    }
                }
                else if (_twoFactorService.IsRequiredForUserType(user.UserType))
                {
                    // 2FA is required but not enabled - prompt user to set it up
                    _logger.LogWarning($"2FA is required for {user.UserType} user {user.Email} but not enabled");
                    return BadRequest(new { 
                        message = "Bu kullanıcı tipi için 2FA zorunludur. Lütfen 2FA'yı etkinleştirin.",
                        requiresTwoFactorSetup = true 
                    });
                }

                // JWT access token oluşturmak için yapılandırma ayarlarını alıyorum.
                var accessToken = JwtHelper.GenerateJwtToken(new JwtDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    UserType = user.UserType,
                    SecretKey = _configuration["Jwt:SecretKey"]!,
                    Issuer = _configuration["Jwt:Issuer"]!,
                    Audience = _configuration["Jwt:Audience"]!,
                    ExpireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"]!)
                });

                // Refresh token oluştur
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, ipAddress);

                // Refresh token'ı yalnızca HttpOnly cookie'de sakla
                SetRefreshCookie(refreshToken);

                // Record successful login attempt
                await _bruteForceProtection.RecordLoginAttemptAsync(request.Email, ipAddress, true, null, user.Id);

                // Token'ların oluşturulduğunu logluyorum.
                _logger.LogInformation($"Token'lar oluşturuldu: Email: {user.Email}, Role: {user.UserType}");
                // Başarı mesajı ve token'ları JSON formatında döndürüyorum.
                return Ok(new LoginResponse
                {
                    Message = "Giriş başarıyla tamamlandı",
                    AccessToken = accessToken
                });
            }
            catch (Exception ex)
            {
                // Eğer bir hata çıkarsa, bunu logluyorum ve 500 hata koduyla hata mesajı döndürüyorum.
                _logger.LogError(ex, $"Giriş sırasında hata çıktı: {request.Email}. InnerException: {ex.InnerException?.Message}");
                return StatusCode(500, new { message = "Giriş sırasında bir hata oluştu." });
            }
        }

        /// <summary>
        /// Giriş yapmış kullanıcının bilgilerini getirir
        /// </summary>
        /// <returns>Kullanıcı bilgileri</returns>
        /// <response code="200">Kullanıcı bilgileri başarıyla getirildi</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <response code="404">Kullanıcı bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpGet("me")]
        [Authorize] // Bu endpoint'e sadece geçerli bir token ile erişilebilir.
        [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyUser()
        {
            try
            {
                // Kullanıcı ID'sini token'dan al
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int personnelId))
                {
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
                }

                // Personel bilgilerini veritabanından çek
                var result = await _personnelService.GetPersonnelById(personnelId);

                if (!result.IsSuccess)
                {
                    return NotFound(new { message = result.Message });
                }

                var user = result.Data;
                if (user is null)
                {
                    return StatusCode(500, new { message = "Kullanıcı bilgisi bulunamadı." });
                }

                // Token'dan ek bilgileri al (varsa) - token'daki bilgiler daha güncel olabilir
                var emailClaim = User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value ?? user.Email;
                var fullNameClaim = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? user.FullName;
                var userTypeClaim = User.Claims.FirstOrDefault(c => c.Type == "UserType")?.Value;

                // UserType'ı parse et
                UserType userType = user.UserType;
                if (!string.IsNullOrEmpty(userTypeClaim) && Enum.TryParse<UserType>(userTypeClaim, out var parsedUserType))
                {
                    userType = parsedUserType;
                }

                return Ok(new UserInfoResponse
                {
                    Id = user.Id,
                    Email = emailClaim,
                    FullName = fullNameClaim,
                    UserType = userType,
                    CreatedDate = user.CreatedDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kullanıcı bilgisi getirilirken hata: {ex.Message}");
                return StatusCode(500, new { message = "Kullanıcı bilgisi getirilirken bir hata oluştu." });
            }
        }

        /// <summary>
        /// Şifre sıfırlama talebi gönderir (email ile token gönderilir)
        /// </summary>
        /// <param name="request">E-posta adresi</param>
        /// <returns>İşlem sonucu</returns>
        /// <response code="200">Şifre sıfırlama talebi başarıyla gönderildi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email))
                {
                    return BadRequest(new { message = "E-posta adresi gereklidir." });
                }

                var result = await _personnelService.RequestPasswordReset(request.Email);
                
                if (result.IsSuccess)
                {
                    return Ok(new { message = result.Message });
                }
                else
                {
                    return BadRequest(new { message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şifre sıfırlama talebi işlenirken hata: {ex.Message}");
                return StatusCode(500, new { message = "Şifre sıfırlama talebi işlenirken bir hata oluştu." });
            }
        }

        /// <summary>
        /// Şifre sıfırlama işlemini tamamlar (token ile yeni şifre belirlenir)
        /// </summary>
        /// <param name="request">Token ve yeni şifre</param>
        /// <returns>İşlem sonucu</returns>
        /// <response code="200">Şifre başarıyla sıfırlandı</response>
        /// <response code="400">Geçersiz token veya istek verisi</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.NewPassword))
                {
                    return BadRequest(new { message = "Token ve yeni şifre gereklidir." });
                }

                var result = await _personnelService.ResetPassword(request.Token, request.NewPassword);
                
                if (result.IsSuccess)
                {
                    return Ok(new { message = result.Message });
                }
                else
                {
                    return BadRequest(new { message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şifre sıfırlanırken hata: {ex.Message}");
                return StatusCode(500, new { message = "Şifre sıfırlanırken bir hata oluştu." });
            }
        }

        /// <summary>
        /// Refresh token ile yeni access token alır
        /// </summary>
        /// <param name="request">Refresh token</param>
        /// <returns>Yeni access token ve refresh token</returns>
        /// <response code="200">Token başarıyla yenilendi</response>
        /// <response code="400">Geçersiz refresh token</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var incomingRefresh = !string.IsNullOrWhiteSpace(request?.RefreshToken)
                    ? request.RefreshToken
                    : Request.Cookies["refreshToken"];

                if (string.IsNullOrWhiteSpace(incomingRefresh))
                    return BadRequest(new { message = "Refresh token gereklidir." });

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var result = await _refreshTokenService.RefreshTokenAsync(incomingRefresh, ipAddress);

                if (result.IsSuccess)
                {
                    // Validate that both tokens are present when IsSuccess is true
                    if (string.IsNullOrWhiteSpace(result.AccessToken) || string.IsNullOrWhiteSpace(result.RefreshToken))
                    {
                        _logger.LogError("RefreshTokenAsync returned IsSuccess=true but tokens are null/empty");
                        return StatusCode(500, new { message = "Token yenilenirken bir hata oluştu." });
                    }

                    // Set refresh token cookie
                    SetRefreshCookie(result.RefreshToken);

                    return Ok(new RefreshTokenResponse
                    {
                        Message = result.Message,
                        AccessToken = result.AccessToken
                    });
                }
                else
                {
                    return BadRequest(new { message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Token yenilenirken hata: {ex.Message}");
                return StatusCode(500, new { message = "Token yenilenirken bir hata oluştu." });
            }
        }

        /// <summary>
        /// Refresh token'ı iptal eder (logout)
        /// </summary>
        /// <param name="request">İptal edilecek refresh token</param>
        /// <returns>İşlem sonucu</returns>
        /// <response code="200">Token başarıyla iptal edildi</response>
        /// <response code="400">Geçersiz token</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpPost("revoke-token")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var incomingRefresh = !string.IsNullOrWhiteSpace(request?.RefreshToken)
                    ? request.RefreshToken
                    : Request.Cookies["refreshToken"];

                if (string.IsNullOrWhiteSpace(incomingRefresh))
                    return BadRequest(new { message = "Refresh token gereklidir." });

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var result = await _refreshTokenService.RevokeTokenAsync(incomingRefresh, ipAddress);

                if (result)
                {
                    Response.Cookies.Delete("refreshToken", new CookieOptions
                    {
                        Path = "/",
                        Secure = true,
                        HttpOnly = true,
                        SameSite = SameSiteMode.None
                    });
                    return Ok(new { message = "Token başarıyla iptal edildi." });
                }
                else
                {
                    return BadRequest(new { message = "Token bulunamadı veya zaten iptal edilmiş." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Token iptal edilirken hata: {ex.Message}");
                return StatusCode(500, new { message = "Token iptal edilirken bir hata oluştu." });
            }
        }

        /// <summary>
        /// Şifre değiştirir (giriş yapmış kullanıcı için)
        /// </summary>
        /// <param name="request">Mevcut şifre ve yeni şifre</param>
        /// <returns>İşlem sonucu</returns>
        /// <response code="200">Şifre başarıyla değiştirildi</response>
        /// <response code="400">Geçersiz şifre veya istek verisi</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                // Kullanıcı ID'sini token'dan al
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int personnelId))
                {
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
                }

                var result = await _personnelService.ChangePassword(
                    personnelId,
                    request.CurrentPassword,
                    request.NewPassword);

                if (result.IsSuccess)
                {
                    // Şifre değiştiğinde tüm refresh token'ları iptal et (güvenlik)
                    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                    await _refreshTokenService.RevokeAllTokensAsync(personnelId, ipAddress);

                    return Ok(new { message = result.Message });
                }
                else
                {
                    return BadRequest(new { message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şifre değiştirilirken hata: {ex.Message}");
                return StatusCode(500, new { message = "Şifre değiştirilirken bir hata oluştu." });
            }
        }

        /// <summary>
        /// Şifre güçlülüğünü kontrol eder ve güç skoru hesaplar
        /// </summary>
        /// <param name="request">Kontrol edilecek şifre</param>
        /// <returns>Şifre doğrulama sonucu ve güç skoru</returns>
        /// <response code="200">Şifre doğrulama sonucu başarıyla döndürüldü</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpPost("validate-password")]
        [ProducesResponseType(typeof(ValidatePasswordResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public IActionResult ValidatePassword([FromBody] ValidatePasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "Şifre gereklidir." });
                }

                var validation = _passwordService.ValidatePassword(request.Password);
                var strengthScore = _passwordService.CalculatePasswordStrength(request.Password);

                string strengthLevel = strengthScore switch
                {
                    >= 80 => "Çok Güçlü",
                    >= 60 => "Güçlü",
                    >= 40 => "Orta",
                    >= 20 => "Zayıf",
                    _ => "Çok Zayıf"
                };

                return Ok(new ValidatePasswordResponse
                {
                    IsValid = validation.IsValid,
                    Message = validation.Message,
                    Errors = validation.Errors,
                    StrengthScore = strengthScore,
                    StrengthLevel = strengthLevel
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şifre doğrulanırken hata: {ex.Message}");
                return StatusCode(500, new { message = "Şifre doğrulanırken bir hata oluştu." });
            }
        }

        /// <summary>
        /// Generate 2FA setup (secret + QR code) for current user
        /// </summary>
        [HttpPost("2fa/setup")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<GuestFlow.Application.Models.Responses.Auth.TwoFactorSetupResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SetupTwoFactor()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int personnelId))
                {
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
                }

                var userResult = await _personnelService.GetPersonnelById(personnelId);
                if (!userResult.IsSuccess || userResult.Data == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı." });
                }

                var issuer = _configuration["Jwt:Issuer"] ?? "GuestFlow";
                var setup = await _twoFactorService.GenerateSetupAsync(personnelId, userResult.Data.Email, issuer);
                
                return Ok(ApiResponse<GuestFlow.Application.Models.Responses.Auth.TwoFactorSetupResponse>.SuccessResponse(setup, "2FA setup başarıyla oluşturuldu."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "2FA setup oluşturulurken hata oluştu.");
                return StatusCode(500, new { message = "2FA setup oluşturulurken bir hata oluştu." });
            }
        }

        /// <summary>
        /// Verify and enable 2FA for current user
        /// </summary>
        [HttpPost("2fa/verify-enable")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> VerifyAndEnableTwoFactor([FromBody] GuestFlow.Application.Models.Responses.Auth.TwoFactorVerifyRequest request)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int personnelId))
                {
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
                }

                var isValid = await _twoFactorService.VerifyAndEnableAsync(personnelId, request.Code);
                if (!isValid)
                {
                    return BadRequest(new { message = "Geçersiz 2FA kodu." });
                }

                return Ok(ApiResponse<bool>.SuccessResponse(true, "2FA başarıyla etkinleştirildi."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "2FA etkinleştirilirken hata oluştu.");
                return StatusCode(500, new { message = "2FA etkinleştirilirken bir hata oluştu." });
            }
        }

        /// <summary>
        /// Disable 2FA for current user (only if not required for user type)
        /// </summary>
        [HttpPost("2fa/disable")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DisableTwoFactor()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int personnelId))
                {
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
                }

                var disabled = await _twoFactorService.DisableAsync(personnelId);
                if (!disabled)
                {
                    return BadRequest(new { message = "2FA devre dışı bırakılamadı. Bu kullanıcı tipi için 2FA zorunludur." });
                }

                return Ok(ApiResponse<bool>.SuccessResponse(true, "2FA başarıyla devre dışı bırakıldı."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "2FA devre dışı bırakılırken hata oluştu.");
                return StatusCode(500, new { message = "2FA devre dışı bırakılırken bir hata oluştu." });
            }
        }

        private void SetRefreshCookie(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return;

            var expireDays = int.TryParse(_configuration["Jwt:RefreshTokenExpireDays"], out var days) ? days : 30;
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(expireDays),
                Path = "/"
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}