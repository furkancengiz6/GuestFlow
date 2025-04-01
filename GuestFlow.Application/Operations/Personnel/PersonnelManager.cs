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

                var hasMail = await _personnelRepository.GetAll(x => x.Email.ToLower() == personnel.Email.ToLower()).AnyAsync();
                if (hasMail)
                    return new ServiceMessage { IsSuccess = false, Message = "Bu mail adresi zaten kayıtlı." };

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

                _logger.LogInformation("Personel eklendi: {Email}", personnel.Email);
                return new ServiceMessage { IsSuccess = true, Message = "Personel başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Personel eklenirken hata oluştu.");
                return new ServiceMessage { IsSuccess = false, Message = "Personel eklenirken hata: " + ex.Message };
            }
        }

        public async Task<ServiceMessage<PersonnelInfoDto>> Login(LoginPersonnelDto login)
        {
            try
            {
                var personnel = await _personnelRepository.GetAll(x => x.Email.ToLower() == login.Email.ToLower())
                    .FirstOrDefaultAsync();

                if (personnel == null)
                    return new ServiceMessage<PersonnelInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Kullanıcı bulunamadı veya şifre hatalı."
                    };

                var unprotectedPassword = _dataProtection.Unprotect(personnel.Password);
                if (unprotectedPassword != login.Password)
                    return new ServiceMessage<PersonnelInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Kullanıcı bulunamadı veya şifre hatalı."
                    };

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
                _logger.LogError(ex, "Giriş yapılırken hata oluştu: {Email}", login.Email);
                return new ServiceMessage<PersonnelInfoDto>
                {
                    IsSuccess = false,
                    Message = "Giriş sırasında hata: " + ex.Message
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

                _logger.LogInformation("Personel silindi: {Id}", id);
                return new ServiceMessage { IsSuccess = true, Message = "Personel başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Personel silinirken hata oluştu.");
                return new ServiceMessage { IsSuccess = false, Message = "Personel silinirken hata: " + ex.Message };
            }
        }
    }
}