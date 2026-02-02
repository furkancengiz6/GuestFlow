using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.AI
{
    /// <summary>
    /// AI tarafından tespit edilen operasyonel komutları işleyen servis arayüzü
    /// </summary>
    public interface IAICommandHandler
    {
        /// <summary>
        /// Belirli bir aksiyon tipi ve parametrelerle komutu yürütür
        /// </summary>
        Task<AICommandResult> ExecuteCommandAsync(string actionType, dynamic parameters);
    }

    public class AICommandResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}
