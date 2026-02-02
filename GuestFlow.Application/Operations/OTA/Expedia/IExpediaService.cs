using GuestFlow.Application.Models.Responses;
using System.Text.Json;

namespace GuestFlow.Application.Operations.OTA.Expedia
{
    public interface IExpediaService
    {
        bool ValidateSignature(string payload, string signature, string secret);
        JsonElement ParsePayload(string payload);
    }
}
