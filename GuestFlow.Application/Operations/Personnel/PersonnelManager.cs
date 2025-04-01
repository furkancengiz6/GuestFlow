using System;
using System.Threading.Tasks;
using GuestFlow.Application.DataProtection;
using GuestFlow.Application.Operations.Personnel.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Personnel
{
    public class PersonnelManager : IPersonnelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IDataProtection _dataProtection;
        private readonly ILogger<PersonnelManager> _logger;

        public PersonnelManager(
            IUnitOfWork unitOfWork,
            IRepository<PersonnelEntity> personnelRepository,
            IDataProtection dataProtection,
            ILogger<PersonnelManager> logger)
        {
            _unitOfWork = unitOfWork;
            _personnelRepository = personnelRepository;
            _dataProtection = dataProtection;
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
                // Kullanıcıyı bul
                var personnel = await _personnelRepository.GetAll(x => x.Email.ToLower() == login.Email.ToLower())
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
    }
}