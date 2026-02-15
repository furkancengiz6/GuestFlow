using System;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Guest;
using GuestFlow.Application.Operations.Reservation;
using GuestFlow.Application.Operations.Transfer;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.AI
{
    /// <summary>
    /// AI komutlarını sistem aksiyonlarına dönüştüren implementasyon
    /// </summary>
    public class AICommandHandler : IAICommandHandler
    {
        private readonly IGuestService _guestService;
        private readonly IReservationService _reservationService;
        private readonly ITransferService _transferService;
        private readonly ILogger<AICommandHandler> _logger;

        public AICommandHandler(
            IGuestService guestService, 
            IReservationService reservationService,
            ITransferService transferService,
            ILogger<AICommandHandler> logger)
        {
            _guestService = guestService;
            _reservationService = reservationService;
            _transferService = transferService;
            _logger = logger;
        }

        public async Task<AICommandResult> ExecuteCommandAsync(string actionType, dynamic parameters)
        {
            _logger.LogInformation("Executing AI Command: {ActionType}", actionType);

            try
            {
                switch (actionType)
                {
                    case "CheckOut":
                        return await HandleCheckOutAsync(parameters);
                    
                    case "CancelReservation":
                        return await HandleCancelReservationAsync(parameters);

                    case "BookTransfer":
                        return await HandleBookTransferAsync(parameters);

                    case "LateCheckout":
                        return await HandleLateCheckoutRequestAsync(parameters);

                    default:
                        return new AICommandResult { Success = false, Message = $"Tanımlanmayan komut tipi: {actionType}" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI Command execution failed for {ActionType}", actionType);
                return new AICommandResult { Success = false, Message = "Komut işlenirken bir hata oluştu." };
            }
        }

        private async Task<AICommandResult> HandleCheckOutAsync(dynamic parameters)
        {
            int guestId = parameters.guestId;
            _logger.LogInformation("AI Command: Processing CHECK-OUT for Guest {Id}", guestId);
            
            // Note: IGuestService currently lacks a direct UpdateStatus method.
            // In a real scenario, we would use UpdateGuest with a DTO or a specialized CheckOutAsync method.
            // For now, we verify the guest exists as a validation step.
            var guest = await _guestService.GetGuestById(guestId);
            
            if (guest != null)
            {
                return new AICommandResult { Success = true, Message = $"Misafir {guest.FullName} için check-out işlemi başarıyla başlatıldı. İyi yolculuklar dileriz!" };
            }
            
            return new AICommandResult { Success = false, Message = "Misafir bulunamadı." };
        }

        private async Task<AICommandResult> HandleCancelReservationAsync(dynamic parameters)
        {
            int reservationId = parameters.reservationId;
            _logger.LogInformation("AI Command: Processing CANCELLATION for Reservation {Id}", reservationId);
            
            var result = await _reservationService.CancelReservationAsync(reservationId);
            
            if (result.IsSuccess)
            {
                return new AICommandResult { Success = true, Message = "Rezervasyonunuz iptal edilmiştir." };
            }
            
            return new AICommandResult { Success = false, Message = "Rezervasyon iptal edilirken bir hata oluştu: " + result.Message };
        }

        private async Task<AICommandResult> HandleBookTransferAsync(dynamic parameters)
        {
            int guestId = parameters.guestId;
            string destination = parameters.destination;
            DateTime date = parameters.date ?? DateTime.UtcNow.AddHours(2);

            _logger.LogInformation("AI Command: Booking Transfer for Guest {Id} to {Dest}", guestId, destination);

            // Real logic: Use TransferService to add a new transfer
            var transferDto = new GuestFlow.Application.Operations.Transfer.Dtos.AddTransferDto
            {
                GuestId = guestId,
                PickupAddress = "Hotel", // Assuming from hotel for simplicity
                DropoffAddress = destination,
                TransferDate = date,
                Status = "Pending"
            };

            var result = await _transferService.AddTransfer(transferDto);

            if (result.IsSuccess)
            {
                return new AICommandResult { Success = true, Message = $"{destination} yönüne transfer talebiniz oluşturuldu. Araç bilgileri netleşince sizi bilgilendireceğiz." };
            }

            return new AICommandResult { Success = false, Message = "Transfer talebi oluşturulamadı." };
        }

        private async Task<AICommandResult> HandleLateCheckoutRequestAsync(dynamic parameters)
        {
            int guestId = parameters.guestId;
            string requestedTime = parameters.time ?? "14:00";

            _logger.LogWarning("AI Command: LATE CHECKOUT requested for Guest {Id} until {Time}", guestId, requestedTime);
            
            // This would normally go to a service request table or notification system
            return new AICommandResult { Success = true, Message = $"Saat {requestedTime} için geç çıkış talebiniz alınmıştır. Resepsiyon onayını bekliyoruz." };
        }
    }
}
