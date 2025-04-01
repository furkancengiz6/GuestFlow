using System;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Guest.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Guest
{
    public class GuestManager : IGuestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly ILogger<GuestManager> _logger;

        public GuestManager(
            IUnitOfWork unitOfWork,
            IRepository<GuestEntity> guestRepository,
            ILogger<GuestManager> logger)
        {
            _unitOfWork = unitOfWork;
            _guestRepository = guestRepository;
            _logger = logger;
        }

        public async Task<ServiceMessage> AddGuest(AddGuestDto guest)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                if (!guest.IsSpecialGuest)
                {
                    if (string.IsNullOrWhiteSpace(guest.Email))
                        return new ServiceMessage { IsSuccess = false, Message = "Email alanı zorunludur." };

                    if (!guest.Email.Contains("@"))
                        return new ServiceMessage { IsSuccess = false, Message = "Geçerli bir e-posta adresi giriniz (örneğin, user@example.com)." };

                    if (guest.Email.Length < 5 || guest.Email.Length > 100)
                        return new ServiceMessage { IsSuccess = false, Message = "Email 5 ila 100 karakter arasında olmalıdır." };

                    if (string.IsNullOrWhiteSpace(guest.PhoneNumber))
                        return new ServiceMessage { IsSuccess = false, Message = "PhoneNumber alanı zorunludur." };

                    if (!guest.PhoneNumber.All(c => char.IsDigit(c) || c == '+'))
                        return new ServiceMessage { IsSuccess = false, Message = "Telefon numarası sadece rakamlardan oluşmalıdır (örneğin, +905551234567)." };

                    if (guest.PhoneNumber.Length < 5 || guest.PhoneNumber.Length > 20)
                        return new ServiceMessage { IsSuccess = false, Message = "PhoneNumber 5 ila 20 karakter arasında olmalıdır." };
                }

                if (guest.IsSpecialGuest)
                {
                    guest.Email ??= "special@guestflow.com";
                    guest.PhoneNumber ??= "+9000000000";
                }

                string guestCode = await GenerateGuestCodeAsync();
                var hasGuest = await _guestRepository.GetAll(x => x.GuestCode == guestCode).AnyAsync();
                if (hasGuest)
                    return new ServiceMessage { IsSuccess = false, Message = "Bu GuestCode ile bir misafir zaten mevcut." };

                var newGuest = new GuestEntity
                {
                    FullName = guest.FullName,
                    Email = guest.Email,
                    PhoneNumber = guest.PhoneNumber,
                    Nationality = guest.Nationality,
                    GuestCode = guestCode,
                    IsSpecialGuest = guest.IsSpecialGuest
                };

                await _guestRepository.AddAsync(newGuest);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Misafir eklendi: {FullName}", guest.FullName);
                return new ServiceMessage { IsSuccess = true, Message = "Misafir başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Misafir eklenirken hata oluştu.");
                return new ServiceMessage { IsSuccess = false, Message = "Misafir eklenirken hata: " + ex.Message };
            }
        }

        public async Task<ServiceMessage> UpdateGuest(UpdateGuestDto guest)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existing = await _guestRepository.GetAsync(x => x.Id == guest.Id);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Misafir bulunamadı." };

                if (!guest.IsSpecialGuest)
                {
                    if (string.IsNullOrWhiteSpace(guest.Email))
                        return new ServiceMessage { IsSuccess = false, Message = "Email alanı zorunludur." };

                    if (!guest.Email.Contains("@"))
                        return new ServiceMessage { IsSuccess = false, Message = "Geçerli bir e-posta adresi giriniz (örneğin, user@example.com)." };

                    if (guest.Email.Length < 5 || guest.Email.Length > 100)
                        return new ServiceMessage { IsSuccess = false, Message = "Email 5 ila 100 karakter arasında olmalıdır." };

                    if (string.IsNullOrWhiteSpace(guest.PhoneNumber))
                        return new ServiceMessage { IsSuccess = false, Message = "PhoneNumber alanı zorunludur." };

                    if (!guest.PhoneNumber.All(c => char.IsDigit(c) || c == '+'))
                        return new ServiceMessage { IsSuccess = false, Message = "Telefon numarası sadece rakamlardan oluşmalıdır (örneğin, +905551234567)." };

                    if (guest.PhoneNumber.Length < 5 || guest.PhoneNumber.Length > 20)
                        return new ServiceMessage { IsSuccess = false, Message = "PhoneNumber 5 ila 20 karakter arasında olmalıdır." };
                }

                existing.FullName = guest.FullName;
                existing.Email = guest.Email ?? "special@guestflow.com";
                existing.PhoneNumber = guest.PhoneNumber ?? "+9000000000";
                existing.Nationality = guest.Nationality;
                existing.IsSpecialGuest = guest.IsSpecialGuest;

                await _guestRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Misafir güncellendi: {Id}", guest.Id);
                return new ServiceMessage { IsSuccess = true, Message = "Misafir başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Misafir güncellenirken hata oluştu.");
                return new ServiceMessage { IsSuccess = false, Message = "Misafir güncellenirken hata: " + ex.Message };
            }
        }

        public async Task<ServiceMessage> DeleteGuest(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _guestRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Misafir silindi: {Id}", id);
                return new ServiceMessage { IsSuccess = true, Message = "Misafir başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Misafir silinirken hata oluştu.");
                return new ServiceMessage { IsSuccess = false, Message = "Misafir silinirken hata: " + ex.Message };
            }
        }

        public async Task<GetGuestDto> GetGuestById(int id)
        {
            try
            {
                var guest = await _guestRepository.GetByIdAsync(id);
                if (guest == null)
                    throw new Exception("Misafir bulunamadı.");

                return new GetGuestDto
                {
                    Id = guest.Id,
                    FullName = guest.FullName,
                    Email = guest.Email,
                    PhoneNumber = guest.PhoneNumber,
                    Nationality = guest.Nationality,
                    GuestCode = guest.GuestCode,
                    IsSpecialGuest = guest.IsSpecialGuest,
                    CreatedDate = guest.CreatedDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Misafir getirilirken hata oluştu: {Id}", id);
                throw;
            }
        }

        public async Task<List<GetGuestDto>> GetGuests()
        {
            try
            {
                var guests = await _guestRepository.GetAll()
                    .Select(g => new GetGuestDto
                    {
                        Id = g.Id,
                        FullName = g.FullName,
                        Email = g.Email,
                        PhoneNumber = g.PhoneNumber,
                        Nationality = g.Nationality,
                        GuestCode = g.GuestCode,
                        IsSpecialGuest = g.IsSpecialGuest,
                        CreatedDate = g.CreatedDate
                    })
                    .ToListAsync();

                return guests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Misafirler listelenirken hata oluştu.");
                throw;
            }
        }

        private async Task<string> GenerateGuestCodeAsync()
        {
            var lastGuest = await _guestRepository.GetAll()
                .OrderByDescending(g => g.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastGuest != null)
            {
                string lastCode = lastGuest.GuestCode;
                string lastNumber = lastCode.Split('-').Last();
                if (int.TryParse(lastNumber, out int number))
                    nextNumber = number + 1;
            }

            return $"GUEST-{nextNumber:D3}";
        }
    }
}