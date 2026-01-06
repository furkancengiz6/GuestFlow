using AutoMapper;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Guest.Dtos;
using GuestFlow.Application.Operations.Notification;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Guest
{
    public class RoomAssignmentManager : IRoomAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<RoomAssignmentEntity> _roomAssignmentRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<HotelEntity> _hotelRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly IRepository<PaymentEntity> _paymentRepository;
        private readonly ILogger<RoomAssignmentManager> _logger;
        private readonly IMapper _mapper;
        private readonly INotificationHubService _hubService;

        public RoomAssignmentManager(
            IUnitOfWork unitOfWork,
            IRepository<RoomAssignmentEntity> roomAssignmentRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<HotelEntity> hotelRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            IRepository<PaymentEntity> paymentRepository,
            ILogger<RoomAssignmentManager> logger,
            IMapper mapper,
            INotificationHubService? hubService = null)
        {
            _unitOfWork = unitOfWork;
            _roomAssignmentRepository = roomAssignmentRepository;
            _guestRepository = guestRepository;
            _hotelRepository = hotelRepository;
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
            _logger = logger;
            _mapper = mapper;
            _hubService = hubService;
        }

        public async Task<ServiceMessage<RoomAssignmentDto>> CreateRoomAssignmentAsync(CreateRoomAssignmentDto request)
        {
            try
            {
                // Validate guest exists
                var guest = await _guestRepository.GetByIdAsync(request.GuestId);
                if (guest == null)
                    return new ServiceMessage<RoomAssignmentDto> { IsSuccess = false, Message = "Misafir bulunamadı." };

                // Validate hotel exists if provided
                if (request.HotelId.HasValue)
                {
                    var hotel = await _hotelRepository.GetByIdAsync(request.HotelId.Value);
                    if (hotel == null)
                        return new ServiceMessage<RoomAssignmentDto> { IsSuccess = false, Message = "Otel bulunamadı." };
                }

                // Check for overlapping assignments for the same guest
                var overlapping = await _roomAssignmentRepository.GetAll()
                    .Where(ra => ra.GuestId == request.GuestId &&
                                ((ra.StartDate <= request.EndDate && ra.EndDate >= request.StartDate) ||
                                 (ra.StartDate <= request.StartDate && (!ra.EndDate.HasValue || ra.EndDate >= request.StartDate))))
                    .AnyAsync();

                if (overlapping)
                    return new ServiceMessage<RoomAssignmentDto> { IsSuccess = false, Message = "Bu tarih aralığında çakışan oda ataması mevcut." };

                var assignment = new RoomAssignmentEntity
                {
                    GuestId = request.GuestId,
                    HotelId = request.HotelId,
                    RoomNumber = request.RoomNumber,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Notes = request.Notes,
                    CreatedByPersonnelId = request.PersonnelId
                };

                await _roomAssignmentRepository.AddAsync(assignment);
                await _unitOfWork.SaveChangesAsync();

                var result = await GetRoomAssignmentDtoAsync(assignment.Id);

                // Send live update for real-time UI updates
                if (_hubService != null)
                {
                    await _hubService.SendLiveUpdateAsync("RoomAssignment", assignment.Id, "created");
                }

                return new ServiceMessage<RoomAssignmentDto> { IsSuccess = true, Message = "Oda ataması başarıyla oluşturuldu.", Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Oda ataması oluşturulurken hata: {ex.Message}");
                return new ServiceMessage<RoomAssignmentDto> { IsSuccess = false, Message = "Oda ataması oluşturulurken hata oluştu." };
            }
        }

        public async Task<ServiceMessage<RoomAssignmentDto>> UpdateRoomAssignmentAsync(UpdateRoomAssignmentDto request)
        {
            try
            {
                var assignment = await _roomAssignmentRepository.GetByIdAsync(request.Id);
                if (assignment == null)
                    return new ServiceMessage<RoomAssignmentDto> { IsSuccess = false, Message = "Oda ataması bulunamadı." };

                // Check for overlapping assignments excluding current
                var overlapping = await _roomAssignmentRepository.GetAll()
                    .Where(ra => ra.Id != request.Id &&
                                ra.GuestId == assignment.GuestId &&
                                ((ra.StartDate <= request.EndDate && ra.EndDate >= request.StartDate) ||
                                 (ra.StartDate <= request.StartDate && (!ra.EndDate.HasValue || ra.EndDate >= request.StartDate))))
                    .AnyAsync();

                if (overlapping)
                    return new ServiceMessage<RoomAssignmentDto> { IsSuccess = false, Message = "Bu tarih aralığında çakışan oda ataması mevcut." };

                assignment.RoomNumber = request.RoomNumber;
                assignment.StartDate = request.StartDate;
                assignment.EndDate = request.EndDate;
                assignment.Notes = request.Notes;
                assignment.MarkAsUpdated(request.PersonnelId);

                await _roomAssignmentRepository.UpdateAsync(assignment);
                await _unitOfWork.SaveChangesAsync();

                var result = await GetRoomAssignmentDtoAsync(assignment.Id);

                // Send live update for real-time UI updates
                if (_hubService != null)
                {
                    await _hubService.SendLiveUpdateAsync("RoomAssignment", assignment.Id, "updated");
                }

                return new ServiceMessage<RoomAssignmentDto> { IsSuccess = true, Message = "Oda ataması başarıyla güncellendi.", Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Oda ataması güncellenirken hata: {ex.Message}");
                return new ServiceMessage<RoomAssignmentDto> { IsSuccess = false, Message = "Oda ataması güncellenirken hata oluştu." };
            }
        }

        public async Task<ServiceMessage<bool>> CloseRoomAssignmentAsync(int assignmentId, CloseRoomAssignmentDto request)
        {
            try
            {
                var assignment = await _roomAssignmentRepository.GetByIdAsync(assignmentId);
                if (assignment == null)
                    return new ServiceMessage<bool> { IsSuccess = false, Message = "Oda ataması bulunamadı." };

                if (assignment.EndDate.HasValue)
                    return new ServiceMessage<bool> { IsSuccess = false, Message = "Bu oda ataması zaten kapatılmış." };

                assignment.EndDate = request.EndDate;
                if (!string.IsNullOrEmpty(request.Notes))
                    assignment.Notes = request.Notes;
                assignment.MarkAsUpdated(request.PersonnelId);

                await _roomAssignmentRepository.UpdateAsync(assignment);
                await _unitOfWork.SaveChangesAsync();

                // Send live update for real-time UI updates
                if (_hubService != null)
                {
                    await _hubService.SendLiveUpdateAsync("RoomAssignment", assignment.Id, "updated"); // "updated" since EndDate changed
                }

                return new ServiceMessage<bool> { IsSuccess = true, Message = "Oda ataması başarıyla kapatıldı.", Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Oda ataması kapatılırken hata: {ex.Message}");
                return new ServiceMessage<bool> { IsSuccess = false, Message = "Oda ataması kapatılırken hata oluştu." };
            }
        }

        public async Task<ServiceMessage<List<RoomAssignmentDto>>> GetGuestRoomAssignmentsAsync(int guestId)
        {
            try
            {
                var assignments = await _roomAssignmentRepository.GetAll()
                    .Where(ra => ra.GuestId == guestId && !ra.IsDeleted)
                    .Include(ra => ra.Guest)
                    .Include(ra => ra.Hotel)
                    .OrderByDescending(ra => ra.StartDate)
                    .ToListAsync();

                var result = _mapper.Map<List<RoomAssignmentDto>>(assignments);
                return new ServiceMessage<List<RoomAssignmentDto>> { IsSuccess = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Misafir oda atamaları getirilirken hata: {ex.Message}");
                return new ServiceMessage<List<RoomAssignmentDto>> { IsSuccess = false, Message = "Misafir oda atamaları getirilirken hata oluştu." };
            }
        }

        public async Task<ServiceMessage<RoomAssignmentDto>> GetCurrentRoomAssignmentAsync(int guestId)
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                var currentAssignment = await _roomAssignmentRepository.GetAll()
                    .Where(ra => ra.GuestId == guestId &&
                                ra.StartDate <= today &&
                                (!ra.EndDate.HasValue || ra.EndDate.Value >= today) &&
                                !ra.IsDeleted)
                    .Include(ra => ra.Guest)
                    .Include(ra => ra.Hotel)
                    .OrderByDescending(ra => ra.StartDate)
                    .FirstOrDefaultAsync();

                if (currentAssignment == null)
                    return new ServiceMessage<RoomAssignmentDto> { IsSuccess = false, Message = "Aktif oda ataması bulunamadı." };

                var result = _mapper.Map<RoomAssignmentDto>(currentAssignment);
                return new ServiceMessage<RoomAssignmentDto> { IsSuccess = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Aktif oda ataması getirilirken hata: GuestId={guestId}");
                return new ServiceMessage<RoomAssignmentDto> { IsSuccess = false, Message = "Aktif oda ataması getirilirken hata oluştu." };
            }
        }

        public async Task<ServiceMessage<RoomContextDto>> GetRoomContextAsync(RoomContextRequestDto request)
        {
            try
            {
                // Find all guest assignments that overlap with the requested date range
                var guestAssignments = await _roomAssignmentRepository.GetAll()
                    .Where(ra => ra.RoomNumber == request.RoomNumber &&
                                (!request.HotelId.HasValue || ra.HotelId == request.HotelId) &&
                                ra.StartDate <= request.EndDate &&
                                (!ra.EndDate.HasValue || ra.EndDate >= request.StartDate) &&
                                !ra.IsDeleted)
                    .Include(ra => ra.Guest)
                    .Include(ra => ra.Hotel)
                    .ToListAsync();

                var guestIds = guestAssignments.Select(ra => ra.GuestId).Distinct().ToList();

                // Get services for these guests within the date range
                var transfers = await GetServicesForGuestsAsync(guestIds, request.StartDate, request.EndDate, "Transfer");
                var cityTours = await GetServicesForGuestsAsync(guestIds, request.StartDate, request.EndDate, "CityTour");
                var yachtTours = await GetServicesForGuestsAsync(guestIds, request.StartDate, request.EndDate, "YachtTour");

                // Get financial summary
                var allServices = transfers.Concat(cityTours).Concat(yachtTours).ToList();
                var financialSummary = await GetRoomFinancialSummaryAsync(allServices);

                var result = new RoomContextDto
                {
                    RoomNumber = request.RoomNumber,
                    HotelName = guestAssignments.FirstOrDefault()?.Hotel?.HotelName ?? "Bilinmiyor",
                    SearchStartDate = request.StartDate,
                    SearchEndDate = request.EndDate,
                    Guests = guestAssignments.Select(ra => new GuestAssignmentDto
                    {
                        GuestId = ra.GuestId,
                        GuestName = ra.Guest.FullName,
                        GuestCode = ra.Guest.GuestCode,
                        AssignmentStart = ra.StartDate,
                        AssignmentEnd = ra.EndDate,
                        Notes = ra.Notes
                    }).ToList(),
                    Transfers = transfers,
                    CityTours = cityTours,
                    YachtTours = yachtTours,
                    FinancialSummary = financialSummary
                };

                return new ServiceMessage<RoomContextDto> { IsSuccess = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Oda bağlamı getirilirken hata: {ex.Message}");
                return new ServiceMessage<RoomContextDto> { IsSuccess = false, Message = "Oda bağlamı getirilirken hata oluştu." };
            }
        }

        private async Task<List<ServiceSummaryDto>> GetServicesForGuestsAsync(
            List<int> guestIds,
            DateTime startDate,
            DateTime endDate,
            string serviceType)
        {
            var services = new List<ServiceSummaryDto>();

            if (serviceType == "Transfer")
            {
                var transfers = await _transferRepository.GetAll()
                    .Where(t => guestIds.Contains(t.GuestId) &&
                               t.TransferDate.Date >= startDate.Date &&
                               t.TransferDate.Date <= endDate.Date &&
                               !t.IsDeleted)
                    .Include(t => t.Guest)
                    .ToListAsync();

                services.AddRange(transfers.Select(t => new ServiceSummaryDto
                {
                    ServiceId = t.Id,
                    ServiceType = "Transfer",
                    Description = $"{t.PickupAddress} → {t.DropoffAddress}",
                    ServiceDate = t.TransferDate,
                    Amount = t.FinalPrice,
                    Currency = t.Currency ?? "TRY",
                    GuestName = t.Guest?.FullName ?? "Bilinmiyor",
                    Status = t.Status.ToString()
                }));
            }
            else if (serviceType == "CityTour")
            {
                var cityTours = await _cityTourRepository.GetAll()
                    .Where(ct => guestIds.Contains(ct.OwnerGuestId) &&
                                ct.TourDate.Date >= startDate.Date &&
                                ct.TourDate.Date <= endDate.Date &&
                                !ct.IsDeleted)
                    .Include(ct => ct.OwnerGuest)
                    .ToListAsync();

                services.AddRange(cityTours.Select(ct => new ServiceSummaryDto
                {
                    ServiceId = ct.Id,
                    ServiceType = "CityTour",
                    Description = $"{ct.DurationHours} saat şehir turu",
                    ServiceDate = ct.TourDate,
                    Amount = ct.FinalPrice,
                    Currency = ct.Currency ?? "TRY",
                    GuestName = ct.OwnerGuest?.FullName ?? "Bilinmiyor",
                    Status = "Confirmed" // CityTour doesn't have status field
                }));
            }
            else if (serviceType == "YachtTour")
            {
                var yachtTours = await _yachtTourRepository.GetAll()
                    .Where(yt => guestIds.Contains(yt.OwnerGuestId) &&
                                yt.TourDate.Date >= startDate.Date &&
                                yt.TourDate.Date <= endDate.Date &&
                                !yt.IsDeleted)
                    .Include(yt => yt.OwnerGuest)
                    .ToListAsync();

                services.AddRange(yachtTours.Select(yt => new ServiceSummaryDto
                {
                    ServiceId = yt.Id,
                    ServiceType = "YachtTour",
                    Description = $"{yt.YachtName ?? "Yat"} turu",
                    ServiceDate = yt.TourDate,
                    Amount = yt.FinalPrice,
                    Currency = yt.Currency ?? "TRY",
                    GuestName = yt.OwnerGuest?.FullName ?? "Bilinmiyor",
                    Status = "Confirmed" // YachtTour doesn't have status field
                }));
            }

            return services.OrderBy(s => s.ServiceDate).ToList();
        }

        private async Task<RoomFinancialSummaryDto> GetRoomFinancialSummaryAsync(List<ServiceSummaryDto> services)
        {
            // Get all service IDs from the services
            var transferIds = services.Where(s => s.ServiceType == "Transfer").Select(s => s.ServiceId).ToList();
            var cityTourIds = services.Where(s => s.ServiceType == "CityTour").Select(s => s.ServiceId).ToList();
            var yachtTourIds = services.Where(s => s.ServiceType == "YachtTour").Select(s => s.ServiceId).ToList();

            // Find invoices that contain these services
            var invoices = await _invoiceRepository.GetAll()
                .Where(i => i.InvoiceItems.Any(item =>
                    (item.ServiceType == "Transfer" && transferIds.Contains(item.ServiceId)) ||
                    (item.ServiceType == "CityTour" && cityTourIds.Contains(item.ServiceId)) ||
                    (item.ServiceType == "YachtTour" && yachtTourIds.Contains(item.ServiceId))))
                .Include(i => i.InvoiceItems)
                .ToListAsync();

            // Get payments related to these invoices
            var invoiceIds = invoices.Select(i => i.Id).ToList();
            var payments = await _paymentRepository.GetAll()
                .Where(p => invoiceIds.Contains(p.InvoiceId ?? 0) && p.Status == Domain.Entities.Enum.PaymentStatus.Completed)
                .ToListAsync();

            return new RoomFinancialSummaryDto
            {
                TotalInvoices = invoices.Count,
                TotalPayments = payments.Count,
                TotalInvoicedAmount = invoices.Sum(i => i.TotalAmount),
                TotalPaidAmount = payments.Sum(p => p.Amount),
                Currency = "TRY" // Simplified - assumes single currency
            };
        }

        private async Task<RoomAssignmentDto> GetRoomAssignmentDtoAsync(int id)
        {
            var assignment = await _roomAssignmentRepository.GetAll()
                .Include(ra => ra.Guest)
                .Include(ra => ra.Hotel)
                .FirstOrDefaultAsync(ra => ra.Id == id);

            return _mapper.Map<RoomAssignmentDto>(assignment);
        }
    }
}
