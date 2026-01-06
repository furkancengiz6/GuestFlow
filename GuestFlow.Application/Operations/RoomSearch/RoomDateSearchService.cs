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

namespace GuestFlow.Application.Operations.RoomSearch
{
    /// <summary>
    /// Room-Date Context Search Service Implementation
    /// 
    /// Enables comprehensive search by Room + Date Range showing all related operations
    /// </summary>
    public class RoomDateSearchService : IRoomDateSearchService
    {
        private readonly IRepository<RoomAssignmentEntity> _roomAssignmentRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly IRepository<PaymentEntity> _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RoomDateSearchService> _logger;

        public RoomDateSearchService(
            IRepository<RoomAssignmentEntity> roomAssignmentRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            IRepository<PaymentEntity> paymentRepository,
            IUnitOfWork unitOfWork,
            ILogger<RoomDateSearchService> logger)
        {
            _roomAssignmentRepository = roomAssignmentRepository;
            _guestRepository = guestRepository;
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<RoomDateSearchResultDto> SearchByRoomAndDateAsync(
            string roomNumber, 
            DateTime startDate, 
            DateTime endDate)
        {
            try
            {
                var result = new RoomDateSearchResultDto
                {
                    RoomNumber = roomNumber,
                    StartDate = startDate,
                    EndDate = endDate
                };

                // Find all room assignments for this room number in the date range
                var roomAssignments = await _roomAssignmentRepository.GetAll()
                    .Include(ra => ra.Guest)
                    .Where(ra => ra.RoomNumber == roomNumber)
                    .Where(ra => ra.StartDate <= endDate &&
                                 (!ra.EndDate.HasValue || ra.EndDate.Value >= startDate))
                    .ToListAsync();

                // Get unique guest IDs from room assignments
                var guestIds = roomAssignments.Select(ra => ra.GuestId).Distinct().ToList();

                // Also include guests who have this room number stored directly
                var directRoomGuests = await _guestRepository.GetAll()
                    .Where(g => g.RoomNumber == roomNumber)
                    .Where(g => (!g.CheckInDate.HasValue || g.CheckInDate <= endDate) &&
                               (!g.CheckOutDate.HasValue || g.CheckOutDate >= startDate))
                    .Select(g => g.Id)
                    .ToListAsync();

                guestIds = guestIds.Union(directRoomGuests).Distinct().ToList();

                // Populate guests
                result.Guests = roomAssignments.Select(ra => new RoomGuestDto
                {
                    Id = ra.GuestId,
                    FullName = ra.Guest.FullName,
                    GuestCode = ra.Guest.GuestCode,
                    CheckInDate = ra.Guest.CheckInDate,
                    CheckOutDate = ra.Guest.CheckOutDate,
                    RoomAssignedDate = ra.StartDate,
                    RoomEndDate = ra.EndDate
                }).DistinctBy(g => g.Id).ToList();

                // Get transfers for these guests in date range
                result.Transfers = await _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .Where(t => guestIds.Contains(t.GuestId))
                    .Where(t => t.TransferDate.Date >= startDate.Date && t.TransferDate.Date <= endDate.Date)
                    .Select(t => new RoomServiceDto
                    {
                        Id = t.Id,
                        ServiceType = "Transfer",
                        ServiceDate = t.TransferDate,
                        GuestName = t.Guest.FullName,
                        Description = $"{t.PickupAddress} → {t.DropoffAddress}",
                        ServiceAmount = t.FinalPrice,
                        Currency = t.Currency ?? "TRY"
                    })
                    .ToListAsync();

                // Get city tours for these guests in date range
                result.CityTours = await _cityTourRepository.GetAll()
                    .Include(ct => ct.OwnerGuest)
                    .Where(ct => guestIds.Contains(ct.OwnerGuestId))
                    .Where(ct => ct.TourDate.Date >= startDate.Date && ct.TourDate.Date <= endDate.Date)
                    .Select(ct => new RoomServiceDto
                    {
                        Id = ct.Id,
                        ServiceType = "CityTour",
                        ServiceDate = ct.TourDate,
                        GuestName = ct.OwnerGuest.FullName,
                        Description = $"Şehir Turu - {ct.Language} ({ct.DurationHours}h)",
                        ServiceAmount = ct.FinalPrice,
                        Currency = ct.Currency ?? "TRY"
                    })
                    .ToListAsync();

                // Get yacht tours for these guests in date range
                result.YachtTours = await _yachtTourRepository.GetAll()
                    .Include(yt => yt.OwnerGuest)
                    .Where(yt => guestIds.Contains(yt.OwnerGuestId))
                    .Where(yt => yt.TourDate.Date >= startDate.Date && yt.TourDate.Date <= endDate.Date)
                    .Select(yt => new RoomServiceDto
                    {
                        Id = yt.Id,
                        ServiceType = "YachtTour",
                        ServiceDate = yt.TourDate,
                        GuestName = yt.OwnerGuest.FullName,
                        Description = $"Yat Turu - {yt.YachtName} ({yt.NumberOfPeople} kişi)",
                        ServiceAmount = yt.FinalPrice,
                        Currency = yt.Currency ?? "TRY"
                    })
                    .ToListAsync();

                // Get invoices for these guests in date range
                result.Invoices = await _invoiceRepository.GetAll()
                    .Include(i => i.Guest)
                    .Where(i => guestIds.Contains(i.GuestId))
                    .Where(i => i.IssueDate.Date >= startDate.Date && i.IssueDate.Date <= endDate.Date)
                    .Select(i => new RoomInvoiceDto
                    {
                        Id = i.Id,
                        InvoiceNumber = i.InvoiceNumber,
                        IssueDate = i.IssueDate,
                        GuestName = i.Guest.FullName,
                        TotalAmount = i.TotalAmount,
                        Currency = i.Currency
                    })
                    .ToListAsync();

                // Get payments for these guests in date range
                result.Payments = await _paymentRepository.GetAll()
                    .Include(p => p.Guest)
                    .Where(p => guestIds.Contains(p.GuestId))
                    .Where(p => p.PaymentDate.Date >= startDate.Date && p.PaymentDate.Date <= endDate.Date)
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .Select(p => new RoomPaymentDto
                    {
                        Id = p.Id,
                        PaymentNumber = p.PaymentNumber,
                        PaymentDate = p.PaymentDate,
                        GuestName = p.Guest.FullName,
                        Amount = p.Amount,
                        Currency = p.Currency,
                        PaymentMethod = p.PaymentMethod.ToString()
                    })
                    .ToListAsync();

                _logger.LogInformation($"Room search completed: Room {roomNumber}, {result.Guests.Count} guests, {result.Transfers.Count} transfers, {result.CityTours.Count} city tours, {result.YachtTours.Count} yacht tours");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Room search error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<RoomAssignmentDto>> GetGuestRoomHistoryAsync(int guestId)
        {
            try
            {
                var assignments = await _roomAssignmentRepository.GetAll()
                    .Include(ra => ra.Guest)
                    .Where(ra => ra.GuestId == guestId)
                    .OrderByDescending(ra => ra.StartDate)
                    .Select(ra => new RoomAssignmentDto
                    {
                        Id = ra.Id,
                        GuestId = ra.GuestId,
                        GuestName = ra.Guest.FullName,
                        RoomNumber = ra.RoomNumber,
                        AssignedDate = ra.StartDate,
                        EndDate = ra.EndDate,
                        Source = ra.Source,
                        Notes = ra.Notes,
                        IsCurrent = !ra.EndDate.HasValue || ra.EndDate.Value > DateTime.UtcNow
                    })
                    .ToListAsync();

                return assignments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get guest room history error: {ex.Message}");
                throw;
            }
        }

        public async Task<RoomAssignmentDto> AddRoomAssignmentAsync(int guestId, string roomNumber, DateTime assignedDate, string source = "Manual")
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // End any current room assignment for this guest
                var currentAssignment = await _roomAssignmentRepository.GetAll()
                    .Where(ra => ra.GuestId == guestId && !ra.EndDate.HasValue)
                    .FirstOrDefaultAsync();

                if (currentAssignment != null)
                {
                    currentAssignment.EndDate = assignedDate;
                    await _roomAssignmentRepository.UpdateAsync(currentAssignment);
                }

                // Create new assignment
                var newAssignment = new RoomAssignmentEntity
                {
                    GuestId = guestId,
                    RoomNumber = roomNumber,
                    StartDate = assignedDate,
                    Source = source
                };

                await _roomAssignmentRepository.AddAsync(newAssignment);

                // Update guest's current room number
                var guest = await _guestRepository.GetByIdAsync(guestId);
                if (guest != null)
                {
                    guest.RoomNumber = roomNumber;
                    await _guestRepository.UpdateAsync(guest);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var guestEntity = await _guestRepository.GetByIdAsync(guestId);

                return new RoomAssignmentDto
                {
                    Id = newAssignment.Id,
                    GuestId = guestId,
                    GuestName = guestEntity?.FullName ?? "",
                    RoomNumber = roomNumber,
                    AssignedDate = assignedDate,
                    Source = source,
                    IsCurrent = true
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Add room assignment error: {ex.Message}");
                throw;
            }
        }

        public async Task EndRoomAssignmentAsync(int assignmentId, DateTime endDate)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var assignment = await _roomAssignmentRepository.GetByIdAsync(assignmentId);
                if (assignment == null)
                    throw new Exception("Room assignment not found");

                assignment.EndDate = endDate;
                await _roomAssignmentRepository.UpdateAsync(assignment);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Room assignment ended: {assignmentId}");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"End room assignment error: {ex.Message}");
                throw;
            }
        }
    }
}

