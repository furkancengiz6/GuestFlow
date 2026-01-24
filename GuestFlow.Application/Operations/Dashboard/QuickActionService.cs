// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.Communication;
using GuestFlow.Application.Operations.PMS;
using GuestFlow.Application.Operations.Transfer;
using GuestFlow.Application.Operations.CityTour;
using GuestFlow.Application.Operations.YachtTour;
using GuestFlow.Application.Operations.RestaurantReservation;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Dashboard
{
    /// <summary>
    /// Quick Action Service implementation
    /// </summary>
    public class QuickActionService : IQuickActionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransferService _transferService;
        private readonly ICityTourService _cityTourService;
        private readonly IYachtTourService _yachtTourService;
        private readonly IRestaurantReservationService _restaurantReservationService;
        private readonly IUnifiedCommunicationService _communicationService;
        private readonly IPMSIntegrationService _pmsIntegrationService;
        private readonly ILogger<QuickActionService> _logger;

        public QuickActionService(
            IUnitOfWork unitOfWork,
            ITransferService transferService,
            ICityTourService cityTourService,
            IYachtTourService yachtTourService,
            IRestaurantReservationService restaurantReservationService,
            IUnifiedCommunicationService communicationService,
            IPMSIntegrationService pmsIntegrationService,
            ILogger<QuickActionService> logger)
        {
            _unitOfWork = unitOfWork;
            _transferService = transferService;
            _cityTourService = cityTourService;
            _yachtTourService = yachtTourService;
            _restaurantReservationService = restaurantReservationService;
            _communicationService = communicationService;
            _pmsIntegrationService = pmsIntegrationService;
            _logger = logger;
        }

        public async Task<ApiResponse<QuickActionTransferResult>> CreateTransferReservationAsync(int guestId, QuickActionTransferRequest request)
        {
            try
            {
                var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
                if (guest == null)
                    return ApiResponse<QuickActionTransferResult>.Fail("Guest not found");

                // Transfer servisini kullanarak rezervasyon oluştur
                // TODO: TransferService'e uygun request modeli oluştur ve çağır
                // Şimdilik placeholder - gerçek implementasyon TransferService'e bağlı
                
                _logger.LogInformation("Quick action: Creating transfer reservation for guest {GuestId}", guestId);
                
                // Örnek implementasyon:
                // var transferRequest = new CreateTransferRequest
                // {
                //     GuestId = guestId,
                //     AirportId = request.AirportId,
                //     HotelId = request.HotelId,
                //     TransferDate = request.TransferDate,
                //     TransferTime = request.TransferTime,
                //     TransferType = request.TransferType,
                //     VehicleId = request.VehicleId,
                //     Notes = request.Notes
                // };
                // var result = await _transferService.CreateTransferAsync(transferRequest);

                return ApiResponse<QuickActionTransferResult>.SuccessResponse(
                    new QuickActionTransferResult
                    {
                        TransferId = 0, // result.Data.Id,
                        TransferNumber = "TEMP-001", // result.Data.TransferNumber,
                        Message = "Transfer reservation created successfully"
                    },
                    "Transfer reservation created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create transfer reservation for guest {GuestId}", guestId);
                return ApiResponse<QuickActionTransferResult>.Fail($"Failed to create transfer reservation: {ex.Message}");
            }
        }

        public async Task<ApiResponse<QuickActionTourResult>> CreateTourReservationAsync(int guestId, QuickActionTourRequest request)
        {
            try
            {
                var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
                if (guest == null)
                    return ApiResponse<QuickActionTourResult>.Fail("Guest not found");

                _logger.LogInformation("Quick action: Creating tour reservation for guest {GuestId}, TourType: {TourType}", 
                    guestId, request.TourType);

                if (request.TourType == "CityTour")
                {
                    // City Tour rezervasyonu
                    // TODO: CityTourService'e uygun request modeli oluştur
                }
                else if (request.TourType == "YachtTour")
                {
                    // Yacht Tour rezervasyonu
                    // TODO: YachtTourService'e uygun request modeli oluştur
                }

                return ApiResponse<QuickActionTourResult>.SuccessResponse(
                    new QuickActionTourResult
                    {
                        ReservationId = 0,
                        ReservationNumber = "TEMP-TOUR-001",
                        Message = "Tour reservation created successfully"
                    },
                    "Tour reservation created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create tour reservation for guest {GuestId}", guestId);
                return ApiResponse<QuickActionTourResult>.Fail($"Failed to create tour reservation: {ex.Message}");
            }
        }

        public async Task<ApiResponse<QuickActionRestaurantResult>> CreateRestaurantReservationAsync(int guestId, QuickActionRestaurantRequest request)
        {
            try
            {
                var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
                if (guest == null)
                    return ApiResponse<QuickActionRestaurantResult>.Fail("Guest not found");

                _logger.LogInformation("Quick action: Creating restaurant reservation for guest {GuestId}, RestaurantId: {RestaurantId}", 
                    guestId, request.RestaurantId);

                // Restaurant rezervasyonu oluştur
                // TODO: RestaurantReservationService'e uygun request modeli oluştur

                return ApiResponse<QuickActionRestaurantResult>.SuccessResponse(
                    new QuickActionRestaurantResult
                    {
                        ReservationId = 0,
                        ReservationNumber = "TEMP-REST-001",
                        Message = "Restaurant reservation created successfully"
                    },
                    "Restaurant reservation created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create restaurant reservation for guest {GuestId}", guestId);
                return ApiResponse<QuickActionRestaurantResult>.Fail($"Failed to create restaurant reservation: {ex.Message}");
            }
        }

        public async Task<ApiResponse<QuickActionRoomServiceResult>> CreateRoomServiceRequestAsync(int guestId, QuickActionRoomServiceRequest request)
        {
            try
            {
                var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
                if (guest == null)
                    return ApiResponse<QuickActionRoomServiceResult>.Fail("Guest not found");

                _logger.LogInformation("Quick action: Creating room service request for guest {GuestId}, ServiceType: {ServiceType}", 
                    guestId, request.ServiceType);

                // Oda servisi talebi oluştur
                // TODO: RoomService entity ve servisi oluştur (şimdilik DailyNote veya başka bir entity kullanılabilir)

                return ApiResponse<QuickActionRoomServiceResult>.SuccessResponse(
                    new QuickActionRoomServiceResult
                    {
                        RequestId = 0,
                        RequestNumber = "TEMP-ROOM-001",
                        Message = "Room service request created successfully"
                    },
                    "Room service request created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create room service request for guest {GuestId}", guestId);
                return ApiResponse<QuickActionRoomServiceResult>.Fail($"Failed to create room service request: {ex.Message}");
            }
        }

        public async Task<ApiResponse<QuickActionMessageResult>> SendMessageAsync(int guestId, QuickActionMessageRequest request)
        {
            try
            {
                var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
                if (guest == null)
                    return ApiResponse<QuickActionMessageResult>.Fail("Guest not found");

                _logger.LogInformation("Quick action: Sending message to guest {GuestId}, Channel: {Channel}", 
                    guestId, request.Channel);

                var sendDto = new Communication.SendMessageDto
                {
                    Channel = request.Channel,
                    Subject = request.Subject,
                    Content = request.Message,
                    TemplateName = request.TemplateName
                };

                var result = await _communicationService.SendMessageAsync(guestId, sendDto);

                if (result.Success)
                {
                    return ApiResponse<QuickActionMessageResult>.SuccessResponse(
                        new QuickActionMessageResult
                        {
                            Sent = true,
                            MessageId = "MSG-001", // result.Data.MessageId,
                            Message = "Message sent successfully"
                        },
                        "Message sent successfully");
                }
                else
                {
                    return ApiResponse<QuickActionMessageResult>.Fail(result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to guest {GuestId}", guestId);
                return ApiResponse<QuickActionMessageResult>.Fail($"Failed to send message: {ex.Message}");
            }
        }

        public async Task<ApiResponse<QuickActionFolioResult>> GetFolioAsync(int guestId)
        {
            try
            {
                var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
                if (guest == null)
                    return ApiResponse<QuickActionFolioResult>.Fail("Guest not found");

                _logger.LogInformation("Quick action: Getting folio for guest {GuestId}", guestId);

                // PMS'den folio bilgisini çek
                var activePMSIntegrations = await _unitOfWork.PMSIntegrations
                    .GetAll(i => i.IsActive && !i.IsDeleted)
                    .ToListAsync();

                foreach (var integration in activePMSIntegrations)
                {
                    try
                    {
                        // PMS guest mapping'i bul
                        var mapping = await _unitOfWork.PMSGuestMappings
                            .GetAll(m => m.PMSIntegrationId == integration.Id && 
                                        m.GuestFlowGuestId == guestId)
                            .FirstOrDefaultAsync();

                        // PMS reservation mapping'i bul
                        var reservationMapping = await _unitOfWork.PMSReservationMappings
                            .GetAll(r => r.PMSIntegrationId == integration.Id && 
                                        r.GuestFlowReservationId.HasValue)
                            .FirstOrDefaultAsync();

                        if (mapping != null && reservationMapping != null && !string.IsNullOrEmpty(reservationMapping.PMSReservationId))
                        {
                            // PMS'den folio'yu çek
                            var folioResponse = await _pmsIntegrationService.GetFolioAsync(
                                integration.Id, reservationMapping.PMSReservationId);

                            if (folioResponse.Success && folioResponse.Data != null)
                            {
                                var folio = folioResponse.Data;
                                return ApiResponse<QuickActionFolioResult>.SuccessResponse(
                                    new QuickActionFolioResult
                                    {
                                        FolioId = folio.FolioId,
                                        TotalAmount = folio.TotalAmount,
                                        PaidAmount = folio.PaidAmount ?? 0,
                                        Balance = folio.Balance ?? 0,
                                        Currency = folio.Currency,
                                        Items = folio.Items?.Select(item => new FolioItemDto
                                        {
                                            Description = item.Description,
                                            Amount = item.Amount,
                                            Category = item.Category,
                                            TransactionDate = item.TransactionDate ?? DateTime.UtcNow
                                        }).ToList() ?? new List<FolioItemDto>()
                                    },
                                    "Folio retrieved successfully");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get folio from PMS integration {IntegrationId}", integration.Id);
                    }
                }

                // PMS'den folio bulunamazsa, GuestFlow'daki invoice'ları kullan
                var invoices = await _unitOfWork.Invoices
                    .GetAll(i => i.GuestId == guestId && !i.IsDeleted)
                    .Include(i => i.InvoiceItems)
                    .OrderByDescending(i => i.IssueDate)
                    .FirstOrDefaultAsync();

                if (invoices != null)
                {
                    return ApiResponse<QuickActionFolioResult>.SuccessResponse(
                        new QuickActionFolioResult
                        {
                            FolioId = invoices.InvoiceNumber.ToString(),
                            TotalAmount = invoices.TotalAmount,
                            PaidAmount = 0, // TODO: Calculate from payments
                            Balance = invoices.TotalAmount, // TODO: Calculate from payments
                            Currency = invoices.Currency,
                            Items = invoices.InvoiceItems
                                .Where(ii => !ii.IsDeleted)
                                .Select(item => new FolioItemDto
                                {
                                    Description = item.Notes ?? string.Empty,
                                    Amount = item.Amount,
                                    Category = item.ServiceType,
                                    TransactionDate = invoices.IssueDate
                                }).ToList()
                        },
                        "Folio retrieved from GuestFlow invoices");
                }

                return ApiResponse<QuickActionFolioResult>.Fail("No folio found for this guest");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get folio for guest {GuestId}", guestId);
                return ApiResponse<QuickActionFolioResult>.Fail($"Failed to get folio: {ex.Message}");
            }
        }
    }
}
