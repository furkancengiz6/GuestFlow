using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.RestaurantReservation.Dtos;
using GuestFlow.Application.Operations.Cache;
using GuestFlow.Application.Operations.Transfer;
using GuestFlow.Application.Operations.Transfer.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.RestaurantReservation
{
    public class RestaurantReservationManager : IRestaurantReservationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<RestaurantReservationEntity> _reservationRepository;
        private readonly IRepository<RestaurantEntity> _restaurantRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly ITransferService _transferService;
        private readonly ILogger<RestaurantReservationManager> _logger;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public RestaurantReservationManager(
            IUnitOfWork unitOfWork,
            IRepository<RestaurantReservationEntity> reservationRepository,
            IRepository<RestaurantEntity> restaurantRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<TransferEntity> transferRepository,
            ITransferService transferService,
            ILogger<RestaurantReservationManager> logger,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _reservationRepository = reservationRepository;
            _restaurantRepository = restaurantRepository;
            _guestRepository = guestRepository;
            _personnelRepository = personnelRepository;
            _transferRepository = transferRepository;
            _transferService = transferService;
            _logger = logger;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<ServiceMessage<GetRestaurantReservationDto>> AddRestaurantReservation(AddRestaurantReservationDto reservation)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var restaurant = await _restaurantRepository.GetByIdAsync(reservation.RestaurantId);
                if (restaurant == null)
                    return new ServiceMessage<GetRestaurantReservationDto> { IsSuccess = false, Message = "Restoran bulunamadı." };

                var guest = await _guestRepository.GetByIdAsync(reservation.GuestId);
                if (guest == null)
                    return new ServiceMessage<GetRestaurantReservationDto> { IsSuccess = false, Message = "Misafir bulunamadı." };

                var personnel = await _personnelRepository.GetByIdAsync(reservation.PersonnelId);
                if (personnel == null)
                    return new ServiceMessage<GetRestaurantReservationDto> { IsSuccess = false, Message = "Personel bulunamadı." };

                // Onay numarası oluştur
                var confirmationNumber = $"REST-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

                var reservationEntity = new RestaurantReservationEntity
                {
                    RestaurantId = reservation.RestaurantId,
                    GuestId = reservation.GuestId,
                    PersonnelId = reservation.PersonnelId,
                    ReservationDate = reservation.ReservationDate,
                    ReservationTime = reservation.ReservationTime,
                    NumberOfGuests = reservation.NumberOfGuests,
                    TableNumber = reservation.TableNumber,
                    SpecialRequests = reservation.SpecialRequests,
                    Status = reservation.Status,
                    ConfirmationNumber = confirmationNumber,
                    Notes = reservation.Notes
                };

                // Otomatik transfer oluştur (otel→restoran)
                if (reservation.CreateTransfer && guest.HotelId.HasValue)
                {
                    var transferDto = new AddTransferDto
                    {
                        TransferDate = reservation.ReservationDate.Date,
                        PickupAddress = $"Otel - {guest.Hotel?.HotelName ?? "Bilinmeyen"}",
                        DropoffAddress = restaurant.Address,
                        Price = 0, // Fiyat sonra belirlenebilir
                        GuestId = reservation.GuestId,
                        PersonnelId = reservation.PersonnelId,
                        TransferType = TransferType.HotelToRestaurant,
                        CreateInvoice = false
                    };

                    var transferResult = await _transferService.AddTransfer(transferDto);
                    if (transferResult.IsSuccess && transferResult.Data != null)
                    {
                        reservationEntity.TransferId = transferResult.Data.TransferId;
                    }
                }

                await _reservationRepository.AddAsync(reservationEntity);
                await _unitOfWork.SaveChangesAsync();

                // Dönüş transferi oluştur (restoran→otel)
                if (reservation.CreateReturnTransfer && guest.HotelId.HasValue)
                {
                    var returnTransferDto = new AddTransferDto
                    {
                        TransferDate = reservation.ReservationDate.Date.AddHours(2), // Rezervasyondan 2 saat sonra
                        PickupAddress = restaurant.Address,
                        DropoffAddress = $"Otel - {guest.Hotel?.HotelName ?? "Bilinmeyen"}",
                        Price = 0,
                        GuestId = reservation.GuestId,
                        PersonnelId = reservation.PersonnelId,
                        TransferType = TransferType.RestaurantToHotel,
                        CreateInvoice = false
                    };

                    var returnTransferResult = await _transferService.AddTransfer(returnTransferDto);
                    if (returnTransferResult.IsSuccess && returnTransferResult.Data != null)
                    {
                        reservationEntity.ReturnTransferId = returnTransferResult.Data.TransferId;
                        await _reservationRepository.UpdateAsync(reservationEntity);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                await _unitOfWork.CommitTransactionAsync();

                var result = await GetRestaurantReservationById(reservationEntity.Id);
                _logger.LogInformation($"Restoran rezervasyonu eklendi: {confirmationNumber}");
                return new ServiceMessage<GetRestaurantReservationDto> { IsSuccess = true, Message = "Restoran rezervasyonu başarıyla oluşturuldu.", Data = result };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Restoran rezervasyonu eklenirken hata: {ex.Message}");
                return new ServiceMessage<GetRestaurantReservationDto> { IsSuccess = false, Message = $"Restoran rezervasyonu eklenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> UpdateRestaurantReservation(UpdateRestaurantReservationDto reservation)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var reservationEntity = await _reservationRepository.GetByIdAsync(reservation.Id);
                if (reservationEntity == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Rezervasyon bulunamadı." };

                reservationEntity.ReservationDate = reservation.ReservationDate;
                reservationEntity.ReservationTime = reservation.ReservationTime;
                reservationEntity.NumberOfGuests = reservation.NumberOfGuests;
                reservationEntity.TableNumber = reservation.TableNumber;
                reservationEntity.SpecialRequests = reservation.SpecialRequests;
                reservationEntity.Status = reservation.Status;
                reservationEntity.TransferId = reservation.TransferId;
                reservationEntity.ReturnTransferId = reservation.ReturnTransferId;
                reservationEntity.Notes = reservation.Notes;

                await _reservationRepository.UpdateAsync(reservationEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Restoran rezervasyonu güncellendi: {reservation.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "Restoran rezervasyonu başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Restoran rezervasyonu güncellenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Restoran rezervasyonu güncellenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> DeleteRestaurantReservation(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var reservationEntity = await _reservationRepository.GetByIdAsync(id);
                if (reservationEntity == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Rezervasyon bulunamadı." };

                await _reservationRepository.DeleteAsync(reservationEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Restoran rezervasyonu silindi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "Restoran rezervasyonu başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Restoran rezervasyonu silinirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Restoran rezervasyonu silinirken hata: {ex.Message}" };
            }
        }

        public async Task<GetRestaurantReservationDto> GetRestaurantReservationById(int id)
        {
            var reservation = await _reservationRepository.GetAll(
                x => x.Id == id,
                x => x.Restaurant,
                x => x.Guest,
                x => x.Personnel)
                .FirstOrDefaultAsync();

            if (reservation == null)
                return null!;

            var dto = new GetRestaurantReservationDto
            {
                Id = reservation.Id,
                RestaurantId = reservation.RestaurantId,
                RestaurantName = reservation.Restaurant?.RestaurantName ?? string.Empty,
                GuestId = reservation.GuestId,
                GuestName = reservation.Guest?.FullName ?? string.Empty,
                PersonnelId = reservation.PersonnelId,
                PersonnelName = reservation.Personnel?.FullName ?? string.Empty,
                ReservationDate = reservation.ReservationDate,
                ReservationTime = reservation.ReservationTime,
                NumberOfGuests = reservation.NumberOfGuests,
                TableNumber = reservation.TableNumber,
                SpecialRequests = reservation.SpecialRequests,
                Status = reservation.Status,
                ConfirmationNumber = reservation.ConfirmationNumber,
                TransferId = reservation.TransferId,
                ReturnTransferId = reservation.ReturnTransferId,
                Notes = reservation.Notes,
                CreatedDate = reservation.CreatedDate
            };

            return dto;
        }

        public async Task<List<GetRestaurantReservationDto>> GetRestaurantReservationsByGuestId(int guestId)
        {
            var reservations = await _reservationRepository.GetAll(
                x => x.GuestId == guestId,
                x => x.Restaurant,
                x => x.Guest,
                x => x.Personnel)
                .OrderByDescending(x => x.ReservationDate)
                .ToListAsync();

            return reservations.Select(r => new GetRestaurantReservationDto
            {
                Id = r.Id,
                RestaurantId = r.RestaurantId,
                RestaurantName = r.Restaurant?.RestaurantName ?? string.Empty,
                GuestId = r.GuestId,
                GuestName = r.Guest?.FullName ?? string.Empty,
                PersonnelId = r.PersonnelId,
                PersonnelName = r.Personnel?.FullName ?? string.Empty,
                ReservationDate = r.ReservationDate,
                ReservationTime = r.ReservationTime,
                NumberOfGuests = r.NumberOfGuests,
                TableNumber = r.TableNumber,
                SpecialRequests = r.SpecialRequests,
                Status = r.Status,
                ConfirmationNumber = r.ConfirmationNumber,
                TransferId = r.TransferId,
                ReturnTransferId = r.ReturnTransferId,
                Notes = r.Notes,
                CreatedDate = r.CreatedDate
            }).ToList();
        }

        public async Task<List<GetRestaurantReservationDto>> GetRestaurantReservationsByRestaurantId(int restaurantId)
        {
            var reservations = await _reservationRepository.GetAll(
                x => x.RestaurantId == restaurantId,
                x => x.Restaurant,
                x => x.Guest,
                x => x.Personnel)
                .OrderBy(x => x.ReservationDate)
                .ThenBy(x => x.ReservationTime)
                .ToListAsync();

            return reservations.Select(r => new GetRestaurantReservationDto
            {
                Id = r.Id,
                RestaurantId = r.RestaurantId,
                RestaurantName = r.Restaurant?.RestaurantName ?? string.Empty,
                GuestId = r.GuestId,
                GuestName = r.Guest?.FullName ?? string.Empty,
                PersonnelId = r.PersonnelId,
                PersonnelName = r.Personnel?.FullName ?? string.Empty,
                ReservationDate = r.ReservationDate,
                ReservationTime = r.ReservationTime,
                NumberOfGuests = r.NumberOfGuests,
                TableNumber = r.TableNumber,
                SpecialRequests = r.SpecialRequests,
                Status = r.Status,
                ConfirmationNumber = r.ConfirmationNumber,
                TransferId = r.TransferId,
                ReturnTransferId = r.ReturnTransferId,
                Notes = r.Notes,
                CreatedDate = r.CreatedDate
            }).ToList();
        }

        public async Task<PagedResult<GetRestaurantReservationDto>> GetRestaurantReservationsPaged(int pageNumber, int pageSize, SortingParameters? sorting = null)
        {
            var query = _reservationRepository.GetAll(null, x => x.Restaurant, x => x.Guest, x => x.Personnel);

            if (sorting != null && !string.IsNullOrEmpty(sorting.SortBy))
            {
                var sortBy = sorting.SortBy.ToLower();
                var sortOrder = sorting.SortOrder?.ToLower() ?? "asc";
                query = sortBy switch
                {
                    "id" => sortOrder == "desc" ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                    "reservationdate" or "date" => sortOrder == "desc" ? query.OrderByDescending(x => x.ReservationDate) : query.OrderBy(x => x.ReservationDate),
                    "reservationtime" or "time" => sortOrder == "desc" ? query.OrderByDescending(x => x.ReservationTime) : query.OrderBy(x => x.ReservationTime),
                    "status" => sortOrder == "desc" ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    "numberofguests" or "guests" => sortOrder == "desc" ? query.OrderByDescending(x => x.NumberOfGuests) : query.OrderBy(x => x.NumberOfGuests),
                    "createddate" or "created" => sortOrder == "desc" ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
                    _ => query.OrderByDescending(x => x.ReservationDate)
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.ReservationDate);
            }

            var totalCount = await query.CountAsync();
            var reservations = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = reservations.Select(r => new GetRestaurantReservationDto
            {
                Id = r.Id,
                RestaurantId = r.RestaurantId,
                RestaurantName = r.Restaurant?.RestaurantName ?? string.Empty,
                GuestId = r.GuestId,
                GuestName = r.Guest?.FullName ?? string.Empty,
                PersonnelId = r.PersonnelId,
                PersonnelName = r.Personnel?.FullName ?? string.Empty,
                ReservationDate = r.ReservationDate,
                ReservationTime = r.ReservationTime,
                NumberOfGuests = r.NumberOfGuests,
                TableNumber = r.TableNumber,
                SpecialRequests = r.SpecialRequests,
                Status = r.Status,
                ConfirmationNumber = r.ConfirmationNumber,
                TransferId = r.TransferId,
                ReturnTransferId = r.ReturnTransferId,
                Notes = r.Notes,
                CreatedDate = r.CreatedDate
            }).ToList();

            return new PagedResult<GetRestaurantReservationDto>
            {
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<ServiceMessage> UpdateRestaurantReservationStatus(int id, ReservationStatus status)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var reservation = await _reservationRepository.GetByIdAsync(id);
                if (reservation == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Rezervasyon bulunamadı." };

                reservation.Status = status;
                await _reservationRepository.UpdateAsync(reservation);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Restoran rezervasyonu durumu güncellendi: {id} -> {status}");
                return new ServiceMessage { IsSuccess = true, Message = "Rezervasyon durumu başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Rezervasyon durumu güncellenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Durum güncellenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> ConfirmRestaurantReservation(int id)
        {
            return await UpdateRestaurantReservationStatus(id, ReservationStatus.Confirmed);
        }

        public async Task<ServiceMessage> CancelRestaurantReservation(int id, string? reason = null)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var reservation = await _reservationRepository.GetByIdAsync(id);
                if (reservation == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Rezervasyon bulunamadı." };

                reservation.Status = ReservationStatus.Cancelled;
                if (!string.IsNullOrEmpty(reason))
                {
                    reservation.Notes = $"İptal nedeni: {reason}. {reservation.Notes}";
                }

                await _reservationRepository.UpdateAsync(reservation);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Restoran rezervasyonu iptal edildi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "Rezervasyon başarıyla iptal edildi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Rezervasyon iptal edilirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Rezervasyon iptal edilirken hata: {ex.Message}" };
            }
        }
    }
}

