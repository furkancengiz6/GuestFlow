using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.OTA.BookingDotCom.Dtos;

namespace GuestFlow.Application.Operations.OTA.BookingDotCom
{
    public interface IBookingDotComService
    {
        Task<ApiResponse<bool>> HandleWebhookAsync(string payload, string signature);
        bool ValidateSignature(string payload, string signature, string secret);
        BookingWebhookPayloadDto ParsePayload(string payload);
    }
}
