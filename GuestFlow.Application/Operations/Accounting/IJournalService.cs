using GuestFlow.Application.Models.Responses.Accounting;

namespace GuestFlow.Application.Operations.Accounting
{
    public interface IJournalService
    {
        Task<ApiResponse<JournalPreviewResponse>> GenerateJournalPreviewAsync(int invoiceId);
        Task<ApiResponse<bool>> PostJournalAsync(JournalPostRequest request);
        Task<ApiResponse<JournalEntryResponse>> GetJournalByInvoiceAsync(int invoiceId);
        
        /// <summary>
        /// Create a reversal entry for an existing journal entry.
        /// POLICY: Unpost is NOT allowed - only reversal entries can be created.
        /// This maintains audit trail integrity.
        /// </summary>
        Task<ApiResponse<JournalEntryResponse>> ReverseJournalEntryAsync(int journalEntryId, string? reversalDescription = null);
    }
}

