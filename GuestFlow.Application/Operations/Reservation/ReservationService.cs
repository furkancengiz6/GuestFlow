using AutoMapper;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Reservation.Dtos;
using GuestFlow.Application.Operations.Validation;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Domain.Events;

namespace GuestFlow.Application.Operations.Reservation
{
    public class ReservationService : IReservationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<ReservationEntity> _reservationRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IForeignKeyValidationService _foreignKeyValidationService;
        private readonly IMapper _mapper;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            IUnitOfWork unitOfWork,
            IRepository<ReservationEntity> reservationRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IForeignKeyValidationService foreignKeyValidationService,
            IMapper mapper,
            ILogger<ReservationService> logger)
        {
            _unitOfWork = unitOfWork;
            _reservationRepository = reservationRepository;
            _guestRepository = guestRepository;
            _personnelRepository = personnelRepository;
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _foreignKeyValidationService = foreignKeyValidationService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceMessage> CreateReservationAsync(AddReservationDto reservation)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Foreign key validasyonları
                var fkValidation = await _foreignKeyValidationService.ValidateMultipleAsync(new ForeignKeyValidationRequest
                {
                    GuestId = reservation.GuestId,
                    PersonnelId = reservation.PersonnelId
                });

                if (!fkValidation.IsValid)
                {
                    return new ServiceMessage { IsSuccess = false, Message = fkValidation.ErrorMessage };
                }

                // Servis tipine göre servis varlığını kontrol et
                var serviceExists = reservation.ServiceType.ToLower() switch
                {
                    "transfer" => await _transferRepository.GetAll(x => x.Id == reservation.ServiceId && !x.IsDeleted).AnyAsync(),
                    "citytour" => await _cityTourRepository.GetAll(x => x.Id == reservation.ServiceId && !x.IsDeleted).AnyAsync(),
                    "yachttour" => await _yachtTourRepository.GetAll(x => x.Id == reservation.ServiceId && !x.IsDeleted).AnyAsync(),
                    _ => false
                };

                if (!serviceExists)
                {
                    return new ServiceMessage { IsSuccess = false, Message = "Belirtilen servis bulunamadı." };
                }

                // Servis bilgilerini al ve toplam tutarı hesapla
                decimal totalAmount = 0;
                string currency = "TRY";

                switch (reservation.ServiceType.ToLower())
                {
                    case "transfer":
                        var transfer = await _transferRepository.GetByIdAsync(reservation.ServiceId);
                        if (transfer != null)
                        {
                            totalAmount = transfer.FinalPrice;
                            currency = transfer.Currency;
                        }
                        break;
                    case "citytour":
                        var cityTour = await _cityTourRepository.GetByIdAsync(reservation.ServiceId);
                        if (cityTour != null)
                        {
                            totalAmount = cityTour.FinalPrice;
                            currency = cityTour.Currency;
                        }
                        break;
                    case "yachttour":
                        var yachtTour = await _yachtTourRepository.GetByIdAsync(reservation.ServiceId);
                        if (yachtTour != null)
                        {
                            totalAmount = yachtTour.FinalPrice;
                            currency = yachtTour.Currency;
                        }
                        break;
                }

                // Rezervasyon numarası oluştur
                var reservationNumber = await GenerateReservationNumberAsync();

                // Rezervasyon entity oluştur
                var reservationEntity = new ReservationEntity
                {
                    ReservationNumber = reservationNumber,
                    GuestId = reservation.GuestId,
                    PersonnelId = reservation.PersonnelId,
                    ServiceType = reservation.ServiceType,
                    ServiceId = reservation.ServiceId,
                    Status = ReservationStatus.Pending,
                    ReservationDate = reservation.ReservationDate,
                    TotalAmount = totalAmount,
                    Currency = currency,
                    Notes = reservation.Notes
                };

                // Add Domain Event
                reservationEntity.AddDomainEvent(new ReservationCreatedEvent(reservationEntity));

                await _reservationRepository.AddAsync(reservationEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Rezervasyon oluşturuldu: {reservationNumber}");
                return new ServiceMessage { IsSuccess = true, Message = "Rezervasyon başarıyla oluşturuldu." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Rezervasyon oluşturulurken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Rezervasyon oluşturulurken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> ConfirmReservationAsync(int reservationId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var reservation = await _reservationRepository.GetAsync(x => x.Id == reservationId && !x.IsDeleted);
                if (reservation == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Rezervasyon bulunamadı." };

                if (reservation.Status == ReservationStatus.Confirmed)
                    return new ServiceMessage { IsSuccess = false, Message = "Rezervasyon zaten onaylanmış." };

                if (reservation.Status == ReservationStatus.Cancelled)
                    return new ServiceMessage { IsSuccess = false, Message = "İptal edilmiş rezervasyon onaylanamaz." };

                reservation.Status = ReservationStatus.Confirmed;
                reservation.ConfirmedDate = DateTime.UtcNow;

                await _reservationRepository.UpdateAsync(reservation);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Rezervasyon onaylandı: {reservation.ReservationNumber}");
                return new ServiceMessage { IsSuccess = true, Message = "Rezervasyon başarıyla onaylandı." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Rezervasyon onaylanırken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Rezervasyon onaylanırken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> CancelReservationAsync(int reservationId, string? cancellationReason = null)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var reservation = await _reservationRepository.GetAsync(x => x.Id == reservationId && !x.IsDeleted);
                if (reservation == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Rezervasyon bulunamadı." };

                if (reservation.Status == ReservationStatus.Cancelled)
                    return new ServiceMessage { IsSuccess = false, Message = "Rezervasyon zaten iptal edilmiş." };

                if (reservation.Status == ReservationStatus.Completed)
                    return new ServiceMessage { IsSuccess = false, Message = "Tamamlanmış rezervasyon iptal edilemez." };

                reservation.Status = ReservationStatus.Cancelled;
                reservation.CancelledDate = DateTime.UtcNow;
                reservation.CancellationReason = cancellationReason;

                await _reservationRepository.UpdateAsync(reservation);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Rezervasyon iptal edildi: {reservation.ReservationNumber}");
                return new ServiceMessage { IsSuccess = true, Message = "Rezervasyon başarıyla iptal edildi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Rezervasyon iptal edilirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Rezervasyon iptal edilirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> UpdateReservationAsync(UpdateReservationDto reservation)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existing = await _reservationRepository.GetAsync(x => x.Id == reservation.Id && !x.IsDeleted);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Rezervasyon bulunamadı." };

                if (existing.Status == ReservationStatus.Cancelled)
                    return new ServiceMessage { IsSuccess = false, Message = "İptal edilmiş rezervasyon güncellenemez." };

                if (existing.Status == ReservationStatus.Completed)
                    return new ServiceMessage { IsSuccess = false, Message = "Tamamlanmış rezervasyon güncellenemez." };

                if (reservation.Notes != null)
                    existing.Notes = reservation.Notes;

                if (reservation.ReservationDate.HasValue)
                    existing.ReservationDate = reservation.ReservationDate.Value;

                await _reservationRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Rezervasyon güncellendi: {existing.ReservationNumber}");
                return new ServiceMessage { IsSuccess = true, Message = "Rezervasyon başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Rezervasyon güncellenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Rezervasyon güncellenirken hata: {ex.Message}" };
            }
        }

        public async Task<GetReservationDto?> GetReservationByIdAsync(int id)
        {
            try
            {
                var reservation = await _reservationRepository.GetAll()
                    .Include(r => r.Guest)
                    .Include(r => r.Personnel)
                    .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

                if (reservation == null)
                    return null;

                return _mapper.Map<GetReservationDto>(reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rezervasyon getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<ReservationDetailDto?> GetReservationDetailAsync(int id)
        {
            try
            {
                var reservation = await _reservationRepository.GetAll()
                    .Include(r => r.Guest)
                    .Include(r => r.Personnel)
                    .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

                if (reservation == null)
                    return null;

                return _mapper.Map<ReservationDetailDto>(reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rezervasyon detayı getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<List<GetReservationDto>> GetReservationsAsync()
        {
            try
            {
                var reservations = await _reservationRepository.GetAll()
                    .Include(r => r.Guest)
                    .Include(r => r.Personnel)
                    .ToListAsync();

                return _mapper.Map<List<GetReservationDto>>(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rezervasyonlar listelenirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<PagedResult<GetReservationDto>> GetReservationsPagedAsync(
            int pageNumber,
            int pageSize,
            ReservationFilterParameters? filters = null,
            SortingParameters? sorting = null)
        {
            try
            {
                var query = _reservationRepository.GetAll()
                    .Include(r => r.Guest)
                    .Include(r => r.Personnel)
                    .ApplyReservationFilters(filters)
                    .ApplyReservationSorting(sorting);

                var totalCount = await query.CountAsync();
                var reservations = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = _mapper.Map<List<GetReservationDto>>(reservations);
                return new PagedResult<GetReservationDto>(dtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sayfalanmış rezervasyonlar listelenirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<List<GetReservationDto>> GetReservationsByGuestIdAsync(int guestId)
        {
            try
            {
                var reservations = await _reservationRepository.GetAll()
                    .Include(r => r.Guest)
                    .Include(r => r.Personnel)
                    .Where(r => r.GuestId == guestId && !r.IsDeleted)
                    .ToListAsync();

                return _mapper.Map<List<GetReservationDto>>(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Misafir rezervasyonları listelenirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<List<GetReservationDto>> GetReservationsByPersonnelIdAsync(int personnelId)
        {
            try
            {
                var reservations = await _reservationRepository.GetAll()
                    .Include(r => r.Guest)
                    .Include(r => r.Personnel)
                    .Where(r => r.PersonnelId == personnelId && !r.IsDeleted)
                    .ToListAsync();

                return _mapper.Map<List<GetReservationDto>>(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Personel rezervasyonları listelenirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<List<GetReservationDto>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var reservations = await _reservationRepository.GetAll()
                    .Include(r => r.Guest)
                    .Include(r => r.Personnel)
                    .Where(r => r.ReservationDate >= startDate && r.ReservationDate <= endDate && !r.IsDeleted)
                    .ToListAsync();

                return _mapper.Map<List<GetReservationDto>>(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Tarih aralığına göre rezervasyonlar listelenirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<List<GetReservationDto>> GetReservationsByStatusAsync(string status)
        {
            try
            {
                var reservationStatus = GuestFlow.Domain.Entities.Enum.ReservationStatusHelper.FromString(status);
                var reservations = await _reservationRepository.GetAll()
                    .Include(r => r.Guest)
                    .Include(r => r.Personnel)
                    .Where(r => r.Status == reservationStatus && !r.IsDeleted)
                    .ToListAsync();

                return _mapper.Map<List<GetReservationDto>>(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Duruma göre rezervasyonlar listelenirken hata: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Benzersiz rezervasyon numarası oluşturur
        /// </summary>
        private async Task<string> GenerateReservationNumberAsync()
        {
            string reservationNumber;
            bool isUnique = false;
            int attempts = 0;
            const int maxAttempts = 10;

            do
            {
                // Format: RES-YYYYMMDD-HHMMSS-XXXX (4 haneli rastgele sayı)
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                var random = new Random().Next(1000, 9999);
                reservationNumber = $"RES-{timestamp}-{random}";

                // Benzersizlik kontrolü
                isUnique = !await _reservationRepository.GetAll(x => x.ReservationNumber == reservationNumber).AnyAsync();
                attempts++;

                if (attempts >= maxAttempts)
                {
                    throw new Exception("Rezervasyon numarası oluşturulamadı. Lütfen tekrar deneyin.");
                }
            } while (!isUnique);

            return reservationNumber;
        }
    }
}

