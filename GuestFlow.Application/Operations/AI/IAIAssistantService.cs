using System.Threading.Tasks;
using GuestFlow.Application.Models.AI;

namespace GuestFlow.Application.Operations.AI
{
    /// <summary>
    /// AI Chatbot asistanı için temel servis arayüzü
    /// </summary>
    public interface IAIAssistantService
    {
        /// <summary>
        /// Gelen mesajı işler ve yapay zeka yanıtı döner
        /// </summary>
        Task<AIChatResponse> ProcessMessageAsync(AIChatRequest request);

        /// <summary>
        /// Belirli bir misafir için bağlamı (context) temizler
        /// </summary>
        Task ClearContextAsync(int guestId);
    }
}
