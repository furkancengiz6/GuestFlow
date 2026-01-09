using GuestFlow.Application.Models.Responses.Accounting;

namespace GuestFlow.Application.Operations.Accounting
{
    public interface IJournalService
    {
        Task<ApiResponse<JournalPreviewResponse>> GenerateJournalPreviewAsync(int invoiceId);
        Task<ApiResponse<bool>> PostJournalAsync(JournalPostRequest request);
    }
}

