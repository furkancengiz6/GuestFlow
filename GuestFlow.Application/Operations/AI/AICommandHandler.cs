using System;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Guest;
using GuestFlow.Application.Operations.Reservation;
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
        private readonly ILogger<AICommandHandler> _logger;

        public AICommandHandler(
            IGuestService guestService, 
            IReservationService reservationService,
            ILogger<AICommandHandler> logger)
        {
            _guestService = guestService;
            _reservationService = reservationService;
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
            // Gerçek sistemde check-out mantığı tetiklenecek
            _logger.LogWarning("AI triggered CHECK-OUT for Guest {Id}", guestId);
            
            // Mock response: Gerçek servis çağrısı eklenebilir
            return new AICommandResult { Success = true, Message = "Check-out işlemi başarıyla başlatıldı." };
        }

        private async Task<AICommandResult> HandleCancelReservationAsync(dynamic parameters)
        {
            int reservationId = parameters.reservationId;
            _logger.LogWarning("AI triggered CANCELLATION for Reservation {Id}", reservationId);
            
            // Mock response
            return new AICommandResult { Success = true, Message = "Rezervasyon iptal talebiniz alındı ve işleniyor." };
        }
    }
}
