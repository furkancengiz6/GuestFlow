using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GuestFlow.Domain.DataProtection;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Email;
using GuestFlow.Application.Operations.Password;
using GuestFlow.Application.Operations.Personnel.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Personnel
{
    public class PersonnelManager : IPersonnelService
    {
        // Token'ları geçici olarak saklamak için in-memory cache (production'da Redis veya ayrı tablo kullanılmalı)
        private static readonly ConcurrentDictionary<string, (int PersonnelId, DateTime Expiry)> _passwordResetTokens = new();

        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IDataProtection _dataProtection;
        private readonly IEmailService _emailService;
        private readonly IPasswordService _passwordService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PersonnelManager> _logger;

        public PersonnelManager(
            IUnitOfWork unitOfWork,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IDataProtection dataProtection,
            IEmailService emailService,
            IPasswordService passwordService,
            IConfiguration configuration,
            ILogger<PersonnelManager> logger)
        {
            _unitOfWork = unitOfWork;
            _personnelRepository = personnelRepository;
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _dataProtection = dataProtection;
            _emailService = emailService;
            _passwordService = passwordService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ServiceMessage> AddPersonnel(AddPersonnelDto personnel)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Validasyon
                if (await _personnelRepository.GetAll(x => x.Email.ToLower() == personnel.Email.ToLower()).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Bu mail adresi zaten kayıtlı." };

                // Şifre güçlülük kontrolü
                var passwordValidation = _passwordService.ValidatePassword(personnel.Password);
                if (!passwordValidation.IsValid)
                {
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = $"Şifre gereksinimleri karşılanmıyor: {string.Join(" ", passwordValidation.Errors)}"
                    };
                }

                // Personel oluşturma
                var personnelEntity = new PersonnelEntity
                {
                    FullName = personnel.FullName,
                    Email = personnel.Email,
                    Password = _dataProtection.Protect(personnel.Password),
                    UserType = UserType.Staff
                };

                await _personnelRepository.AddAsync(personnelEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Personel eklendi: {personnel.Email}");
                return new ServiceMessage { IsSuccess = true, Message = "Personel başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Personel eklenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Personel eklenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        public async Task<ServiceMessage<PersonnelInfoDto>> Login(LoginPersonnelDto login)
        {
            try
            {
                // Kullanıcıyı bul - Giriş aşamasında TenantId henüz belirlenmediği için filtreleri yoksayıyoruz
        var personnel = await _personnelRepository.GetAll(x => x.Email.ToLower() == login.Email.ToLower(), includeDeleted: true)
            .FirstOrDefaultAsync();

                if (personnel == null)
                    return new ServiceMessage<PersonnelInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Kullanıcı bulunamadı veya şifre hatalı."
                    };

                // Şifreyi kontrol et
                var unprotectedPassword = _dataProtection.Unprotect(personnel.Password);
                if (unprotectedPassword != login.Password)
                    return new ServiceMessage<PersonnelInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Kullanıcı bulunamadı veya şifre hatalı."
                    };

                // Başarılı giriş
                return new ServiceMessage<PersonnelInfoDto>
                {
                    IsSuccess = true,
                    Message = "Giriş başarılı.",
                    Data = new PersonnelInfoDto
                    {
                        Id = personnel.Id,
                        TenantId = personnel.TenantId,
                        Email = personnel.Email,
                        FullName = personnel.FullName,
                        UserType = personnel.UserType
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Giriş yapılırken hata: {ex.Message}. Email: {login.Email}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Giriş sırasında hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage<PersonnelInfoDto>
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
        }

        public async Task<ServiceMessage<PersonnelInfoDto>> GetPersonnelById(int id)
        {
            try
            {
                var personnel = await _personnelRepository.GetByIdAsync(id);

                if (personnel == null)
                {
                    return new ServiceMessage<PersonnelInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Personel bulunamadı."
                    };
                }

                return new ServiceMessage<PersonnelInfoDto>
                {
                    IsSuccess = true,
                    Message = "Personel bilgileri başarıyla getirildi.",
                    Data = new PersonnelInfoDto
                    {
                        Id = personnel.Id,
                        TenantId = personnel.TenantId,
                        Email = personnel.Email,
                        FullName = personnel.FullName,
                        UserType = personnel.UserType,
                        CreatedDate = personnel.CreatedDate
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Personel bilgisi getirilirken hata: {ex.Message}. Id: {id}");
                return new ServiceMessage<PersonnelInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Personel bilgisi getirilirken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<List<PersonnelInfoDto>>> GetAllPersonnel()
        {
            try
            {
                // Development ortamında tüm personelleri listele (TenantId filtresini yoksay)
                var query = string.Equals(_configuration["ASPNETCORE_ENVIRONMENT"], "Development", StringComparison.OrdinalIgnoreCase)
                    ? _personnelRepository.GetAll(null, includeDeleted: true)
                    : _personnelRepository.GetAll();

                var personnelList = await query
                    .Select(p => new PersonnelInfoDto
                    {
                        Id = p.Id,
                        TenantId = p.TenantId,
                        Email = p.Email,
                        FullName = p.FullName,
                        UserType = p.UserType,
                        CreatedDate = p.CreatedDate
                    })
                    .ToListAsync();

                return new ServiceMessage<List<PersonnelInfoDto>>
                {
                    IsSuccess = true,
                    Message = "Personel listesi başarıyla getirildi.",
                    Data = personnelList
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Personel listesi getirilirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Personel listesi getirilirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage<List<PersonnelInfoDto>>
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
        }

        public async Task<ServiceMessage> UpdatePersonnel(UpdatePersonnelDto updatePersonnel)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Personeli bul
                var personnel = await _personnelRepository.GetByIdAsync(updatePersonnel.Id);
                if (personnel == null)
                {
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = "Personel bulunamadı."
                    };
                }

                // E-posta değişiyorsa, başka bir personelde kullanılıyor mu kontrol et
                if (personnel.Email.ToLower() != updatePersonnel.Email.ToLower())
                {
                    var emailExists = await _personnelRepository.GetAll(x => x.Email.ToLower() == updatePersonnel.Email.ToLower() && x.Id != updatePersonnel.Id)
                        .AnyAsync();
                    if (emailExists)
                    {
                        return new ServiceMessage
                        {
                            IsSuccess = false,
                            Message = "Bu e-posta adresi başka bir personel tarafından kullanılıyor."
                        };
                    }
                }

                // Bilgileri güncelle
                personnel.FullName = updatePersonnel.FullName;
                personnel.Email = updatePersonnel.Email;
                if (updatePersonnel.UserType.HasValue)
                {
                    personnel.UserType = updatePersonnel.UserType.Value;
                }

                // Şifre değiştiriliyorsa
                if (!string.IsNullOrEmpty(updatePersonnel.NewPassword))
                {
                    // Şifre güçlülük kontrolü
                    var passwordValidation = _passwordService.ValidatePassword(updatePersonnel.NewPassword);
                    if (!passwordValidation.IsValid)
                    {
                        return new ServiceMessage
                        {
                            IsSuccess = false,
                            Message = $"Şifre gereksinimleri karşılanmıyor: {string.Join(" ", passwordValidation.Errors)}"
                        };
                    }
                    personnel.Password = _dataProtection.Protect(updatePersonnel.NewPassword);
                }

                await _personnelRepository.UpdateAsync(personnel);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Personel güncellendi: {personnel.Email}");
                return new ServiceMessage
                {
                    IsSuccess = true,
                    Message = "Personel başarıyla güncellendi."
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Personel güncellenirken hata: {ex.Message}. Id: {updatePersonnel.Id}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Personel güncellenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = errorMessage
                };
            }
        }

        public async Task<ServiceMessage> DeletePersonnel(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _personnelRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Personel silindi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "Personel başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Personel silinirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Personel silinirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        public async Task<ServiceMessage<string>> RequestPasswordReset(string email)
        {
            try
            {
                var personnel = await _personnelRepository.GetAll(x => x.Email.ToLower() == email.ToLower())
                    .FirstOrDefaultAsync();

                if (personnel == null)
                {
                    // Güvenlik için kullanıcı bulunamadı mesajı vermiyoruz
                    _logger.LogWarning($"Şifre sıfırlama talebi: Kullanıcı bulunamadı - {email}");
                    return new ServiceMessage<string>
                    {
                        IsSuccess = true,
                        Message = "Eğer bu e-posta adresi kayıtlıysa, şifre sıfırlama linki gönderilmiştir."
                    };
                }

                // Token oluştur (24 saat geçerli)
                var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .Replace("=", "");

                // Token'ı in-memory cache'e kaydet (production'da Redis veya ayrı tablo kullanılmalı)
                var expiry = DateTime.UtcNow.AddHours(24);
                _passwordResetTokens[token] = (personnel.Id, expiry);

                // E-posta gönder
                await _emailService.SendPasswordResetEmailAsync(personnel.Email, personnel.FullName, token);

                _logger.LogInformation($"Şifre sıfırlama token'ı oluşturuldu: {email}");
                return new ServiceMessage<string>
                {
                    IsSuccess = true,
                    Message = "Eğer bu e-posta adresi kayıtlıysa, şifre sıfırlama linki gönderilmiştir.",
                    Data = token // Sadece test için, production'da gönderilmemeli
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şifre sıfırlama talebi işlenirken hata: {ex.Message}. Email: {email}");
                return new ServiceMessage<string>
                {
                    IsSuccess = false,
                    Message = "Şifre sıfırlama talebi işlenirken bir hata oluştu."
                };
            }
        }

        public async Task<ServiceMessage> ResetPassword(string token, string newPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword))
                {
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = "Token ve yeni şifre gereklidir."
                    };
                }

                // Token'ı kontrol et
                if (!_passwordResetTokens.TryGetValue(token, out var tokenInfo))
                {
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = "Geçersiz token."
                    };
                }

                // Token süresi dolmuş mu kontrol et
                if (tokenInfo.Expiry < DateTime.UtcNow)
                {
                    _passwordResetTokens.TryRemove(token, out _);
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = "Token süresi dolmuş."
                    };
                }

                // Personeli bul
                var targetPersonnel = await _personnelRepository.GetByIdAsync(tokenInfo.PersonnelId);
                if (targetPersonnel == null)
                {
                    _passwordResetTokens.TryRemove(token, out _);
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = "Kullanıcı bulunamadı."
                    };
                }

                // Şifre güçlülük kontrolü
                var passwordValidation = _passwordService.ValidatePassword(newPassword);
                if (!passwordValidation.IsValid)
                {
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = $"Şifre gereksinimleri karşılanmıyor: {string.Join(" ", passwordValidation.Errors)}"
                    };
                }

                // Şifreyi güncelle
                await _unitOfWork.BeginTransactionAsync();
                targetPersonnel.Password = _dataProtection.Protect(newPassword);
                await _personnelRepository.UpdateAsync(targetPersonnel);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Token'ı temizle
                _passwordResetTokens.TryRemove(token, out _);

                _logger.LogInformation($"Şifre sıfırlandı: {targetPersonnel.Email}");
                return new ServiceMessage
                {
                    IsSuccess = true,
                    Message = "Şifre başarıyla sıfırlandı."
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Şifre sıfırlanırken hata: {ex.Message}");
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = "Şifre sıfırlanırken bir hata oluştu."
                };
            }
        }

        public async Task<ServiceMessage> ChangePassword(int personnelId, string currentPassword, string newPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
                {
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = "Mevcut şifre ve yeni şifre gereklidir."
                    };
                }

                // Mevcut şifre ile yeni şifre aynı mı kontrol et
                if (currentPassword == newPassword)
                {
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = "Yeni şifre mevcut şifre ile aynı olamaz."
                    };
                }

                // Personeli bul
                var personnel = await _personnelRepository.GetByIdAsync(personnelId);
                if (personnel == null)
                {
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = "Kullanıcı bulunamadı."
                    };
                }

                // Mevcut şifreyi kontrol et
                var unprotectedPassword = _dataProtection.Unprotect(personnel.Password);
                if (unprotectedPassword != currentPassword)
                {
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = "Mevcut şifre hatalı."
                    };
                }

                // Şifre güçlülük kontrolü
                var passwordValidation = _passwordService.ValidatePassword(newPassword);
                if (!passwordValidation.IsValid)
                {
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = $"Şifre gereksinimleri karşılanmıyor: {string.Join(" ", passwordValidation.Errors)}"
                    };
                }

                // Şifreyi güncelle
                await _unitOfWork.BeginTransactionAsync();
                personnel.Password = _dataProtection.Protect(newPassword);
                await _personnelRepository.UpdateAsync(personnel);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Şifre değiştirildi: PersonnelId: {personnelId}");
                return new ServiceMessage
                {
                    IsSuccess = true,
                    Message = "Şifre başarıyla değiştirildi."
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Şifre değiştirilirken hata: {ex.Message}. PersonnelId: {personnelId}");
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = "Şifre değiştirilirken bir hata oluştu."
                };
            }
        }

        public async Task<PersonnelDetailDto> GetPersonnelDetailAsync(int id)
        {
            try
            {
                var personnel = await _personnelRepository.GetByIdAsync(id);
                if (personnel == null)
                    throw new Exception("Personel bulunamadı.");

                var now = DateTime.UtcNow;

                // Transfer istatistikleri
                var transfers = await _transferRepository.GetAll()
                    .Where(t => t.PersonnelId == id && !t.IsDeleted)
                    .ToListAsync();

                var cityTours = await _cityTourRepository.GetAll()
                    .Where(ct => ct.PersonnelId == id && !ct.IsDeleted)
                    .ToListAsync();

                var yachtTours = await _yachtTourRepository.GetAll()
                    .Where(yt => yt.PersonnelId == id && !yt.IsDeleted)
                    .ToListAsync();

                var totalTransfers = transfers.Count;
                var totalCityTours = cityTours.Count;
                var totalYachtTours = yachtTours.Count;
                var totalBookings = totalTransfers + totalCityTours + totalYachtTours;
                var totalRevenue = transfers.Sum(t => t.FinalPrice) +
                                  cityTours.Sum(ct => ct.FinalPrice) +
                                  yachtTours.Sum(yt => yt.FinalPrice);
                var averageBookingValue = totalBookings > 0 ? totalRevenue / totalBookings : 0;
                var completedBookings = transfers.Count(t => t.TransferDate < now) +
                                       cityTours.Count(ct => ct.TourDate < now) +
                                       yachtTours.Count(yt => yt.TourDate < now);
                var pendingBookings = totalBookings - completedBookings;

                // Son aktiviteler
                var recentActivities = new List<PersonnelActivityDto>();

                var recentTransfers = transfers
                    .OrderByDescending(t => t.TransferDate)
                    .Take(10)
                    .Select(t => new PersonnelActivityDto
                    {
                        Id = t.Id,
                        ActivityType = "Transfer",
                        Description = $"{t.PickupAddress} → {t.DropoffAddress}",
                        ActivityDate = t.TransferDate,
                        GuestName = null, // Guest bilgisi lazy loading ile gelmiyor, ayrı sorgu gerekir
                        Amount = t.FinalPrice,
                        Status = t.Status
                    })
                    .ToList();

                var recentCityTours = cityTours
                    .OrderByDescending(ct => ct.TourDate)
                    .Take(10)
                    .Select(ct => new PersonnelActivityDto
                    {
                        Id = ct.Id,
                        ActivityType = "CityTour",
                        Description = $"Şehir Turu - {ct.Language}",
                        ActivityDate = ct.TourDate,
                        GuestName = null,
                        Amount = ct.FinalPrice,
                        Status = "Aktif"
                    })
                    .ToList();

                var recentYachtTours = yachtTours
                    .OrderByDescending(yt => yt.TourDate)
                    .Take(10)
                    .Select(yt => new PersonnelActivityDto
                    {
                        Id = yt.Id,
                        ActivityType = "YachtTour",
                        Description = $"Yat Turu - {yt.YachtName}",
                        ActivityDate = yt.TourDate,
                        GuestName = null,
                        Amount = yt.FinalPrice,
                        Status = "Aktif"
                    })
                    .ToList();

                recentActivities = recentTransfers
                    .Concat(recentCityTours)
                    .Concat(recentYachtTours)
                    .OrderByDescending(a => a.ActivityDate)
                    .Take(20)
                    .ToList();

                var detail = new PersonnelDetailDto
                {
                    Id = personnel.Id,
                    Email = personnel.Email,
                    FullName = personnel.FullName,
                    UserType = personnel.UserType,
                    CreatedDate = personnel.CreatedDate,
                    Statistics = new PersonnelStatisticsDto
                    {
                        TotalTransfers = totalTransfers,
                        TotalCityTours = totalCityTours,
                        TotalYachtTours = totalYachtTours,
                        TotalBookings = totalBookings,
                        TotalRevenue = totalRevenue,
                        AverageBookingValue = averageBookingValue,
                        CompletedBookings = completedBookings,
                        PendingBookings = pendingBookings
                    },
                    RecentActivities = recentActivities
                };

                return detail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Personel detayı getirilirken hata: {ex.Message}. Id: {id}");
                throw;
            }
        }

        public async Task<PagedResult<PersonnelInfoDto>> GetPersonnelPagedAsync(int pageNumber, int pageSize, PersonnelFilterParameters? filters = null, SortingParameters? sorting = null)
        {
            try
            {
                var query = _personnelRepository.GetAll()
                    .ApplyPersonnelFilters(filters)
                    .ApplyPersonnelSorting(sorting)
                    .Select(p => new PersonnelInfoDto
                    {
                        Id = p.Id,
                        TenantId = p.TenantId,
                        Email = p.Email,
                        FullName = p.FullName,
                        UserType = p.UserType,
                        CreatedDate = p.CreatedDate
                    });

                return await query.ToPagedResultAsync(pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sayfalanmış personeller listelenirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<List<PersonnelActivityDto>> GetPersonnelActivitiesAsync(int id, int? limit = 20)
        {
            try
            {
                var limitValue = limit ?? 20;

                var transfers = await _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .Where(t => t.PersonnelId == id && !t.IsDeleted)
                    .OrderByDescending(t => t.TransferDate)
                    .Take(limitValue)
                    .Select(t => new PersonnelActivityDto
                    {
                        Id = t.Id,
                        ActivityType = "Transfer",
                        Description = $"{t.PickupAddress} → {t.DropoffAddress}",
                        ActivityDate = t.TransferDate,
                        GuestName = t.Guest != null ? t.Guest.FullName : null,
                        Amount = t.FinalPrice,
                        Status = t.Status
                    })
                    .ToListAsync();

                var cityTours = await _cityTourRepository.GetAll()
                    .Include(ct => ct.OwnerGuest)
                    .Where(ct => ct.PersonnelId == id && !ct.IsDeleted)
                    .OrderByDescending(ct => ct.TourDate)
                    .Take(limitValue)
                    .Select(ct => new PersonnelActivityDto
                    {
                        Id = ct.Id,
                        ActivityType = "CityTour",
                        Description = $"Şehir Turu - {ct.Language}",
                        ActivityDate = ct.TourDate,
                        GuestName = ct.OwnerGuest != null ? ct.OwnerGuest.FullName : null,
                        Amount = ct.FinalPrice,
                        Status = "Aktif"
                    })
                    .ToListAsync();

                var yachtTours = await _yachtTourRepository.GetAll()
                    .Include(yt => yt.OwnerGuest)
                    .Where(yt => yt.PersonnelId == id && !yt.IsDeleted)
                    .OrderByDescending(yt => yt.TourDate)
                    .Take(limitValue)
                    .Select(yt => new PersonnelActivityDto
                    {
                        Id = yt.Id,
                        ActivityType = "YachtTour",
                        Description = $"Yat Turu - {yt.YachtName}",
                        ActivityDate = yt.TourDate,
                        GuestName = yt.OwnerGuest != null ? yt.OwnerGuest.FullName : null,
                        Amount = yt.FinalPrice,
                        Status = "Aktif"
                    })
                    .ToListAsync();

                var activities = transfers
                    .Concat(cityTours)
                    .Concat(yachtTours)
                    .OrderByDescending(a => a.ActivityDate)
                    .Take(limitValue)
                    .ToList();

                return activities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Personel aktiviteleri getirilirken hata: {ex.Message}. Id: {id}");
                throw;
            }
        }
    }
}