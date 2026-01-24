// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.Transfer;
using GuestFlow.Application.Operations.Payment;
using GuestFlow.Application.Operations.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Günlük operasyonlar için hızlı aksiyon endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/operations")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff,Concierge,Manager")]
    [Tags("Operations")]
    public class OperationsController : BaseController
    {
        private readonly ITransferService _transferService;
        private readonly IPaymentService _paymentService;
        private readonly INotificationHubService _notificationHubService;

        public OperationsController(
            ITransferService transferService,
            IPaymentService paymentService,
            INotificationHubService notificationHubService)
        {
            _transferService = transferService;
            _paymentService = paymentService;
            _notificationHubService = notificationHubService;
        }

        /// <summary>
        /// Transfer'i onaylar
        /// </summary>
        [HttpPost("transfers/{id}/confirm")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmTransfer(int id)
        {
            try
            {
                var result = await _transferService.UpdateTransferStatusAsync(id, "Confirmed");
                
                // Send SignalR update
                if (result.IsSuccess)
                {
                    await _notificationHubService.SendDailyOperationsUpdateAsync("ServiceUpdated", new { ServiceType = "Transfer", ServiceId = id, Status = "Confirmed" });
                }
                
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                return Error("Transfer onaylanırken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Transfer'i iptal eder
        /// </summary>
        [HttpPost("transfers/{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CancelTransfer(int id, [FromBody] CancelTransferRequest? request = null)
        {
            try
            {
                var result = await _transferService.UpdateTransferStatusAsync(id, "Cancelled");
                
                // Send SignalR update
                if (result.IsSuccess)
                {
                    await _notificationHubService.SendDailyOperationsUpdateAsync("ServiceUpdated", new { ServiceType = "Transfer", ServiceId = id, Status = "Cancelled" });
                }
                
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                return Error("Transfer iptal edilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Transfer'e şoför atar
        /// </summary>
        [HttpPost("transfers/{id}/assign-driver")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignDriver(int id, [FromBody] AssignDriverRequest request)
        {
            try
            {
                // TransferManager'da AssignPersonnelAsync yok, UpdateTransfer kullan
                var transfer = await _transferService.GetTransferById(id);
                if (transfer == null)
                    return Error("Transfer bulunamadı.", 404);

                var updateDto = new GuestFlow.Application.Operations.Transfer.Dtos.UpdateTransferDto
                {
                    Id = id,
                    PersonnelId = request.PersonnelId,
                    // Diğer alanları mevcut değerlerle doldur
                    PickupAddress = transfer.PickupAddress,
                    DropoffAddress = transfer.DropoffAddress,
                    TransferDate = transfer.TransferDate,
                    Price = transfer.Price,
                    Currency = transfer.Currency,
                    GuestId = transfer.GuestId,
                };

                var result = await _transferService.UpdateTransfer(updateDto);
                
                // Send SignalR update
                if (result.IsSuccess)
                {
                    await _notificationHubService.SendDailyOperationsUpdateAsync("DriverAssigned", new { ServiceType = "Transfer", ServiceId = id, PersonnelId = request.PersonnelId });
                }
                
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                return Error("Şoför atanırken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Servis için ödeme alır
        /// </summary>
        [HttpPost("services/{serviceType}/{serviceId}/payment")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RecordPayment(
            string serviceType,
            int serviceId,
            [FromBody] RecordPaymentRequest request)
        {
            try
            {
                // Service type'a göre payment oluştur
                // Get current user ID for CollectedByPersonnelId
                var currentUserIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out var currentUserId))
                {
                    return Error("Kullanıcı bilgisi bulunamadı.", 401);
                }

                var paymentDto = new GuestFlow.Application.Operations.Payment.Dtos.AddPaymentDto
                {
                    GuestId = request.GuestId,
                    CollectedByPersonnelId = currentUserId,
                    Amount = request.Amount,
                    Currency = request.Currency ?? "TRY",
                    PaymentMethod = request.PaymentMethod,
                    PaymentDate = request.PaymentDate ?? DateTime.UtcNow,
                    Notes = request.Notes,
                };

                // Service type'a göre ID'yi set et
                switch (serviceType.ToLower())
                {
                    case "transfer":
                        paymentDto.TransferId = serviceId;
                        break;
                    case "citytour":
                        paymentDto.CityTourId = serviceId;
                        break;
                    case "yachttour":
                        paymentDto.YachtTourId = serviceId;
                        break;
                    default:
                        return Error("Geçersiz servis tipi.", 400);
                }

                var result = await _paymentService.AddPaymentAsync(paymentDto);
                
                // Send SignalR update
                if (result.IsSuccess)
                {
                    await _notificationHubService.SendDailyOperationsUpdateAsync("PaymentRecorded", new { ServiceType = serviceType, ServiceId = serviceId, Amount = request.Amount, Currency = request.Currency ?? "TRY" });
                }
                
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                return Error("Ödeme kaydedilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }

    public class CancelTransferRequest
    {
        public string? Reason { get; set; }
    }

    public class AssignDriverRequest
    {
        public int PersonnelId { get; set; }
    }

    public class RecordPaymentRequest
    {
        public int GuestId { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public DateTime? PaymentDate { get; set; }
        public string? Notes { get; set; }
    }

}
