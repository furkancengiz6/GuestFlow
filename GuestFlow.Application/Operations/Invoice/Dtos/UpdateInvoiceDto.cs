namespace GuestFlow.Application.Operations.Invoice.Dtos
{
    /// <summary>
    /// DTO for updating invoices (limited fields for immutability)
    /// INVOICE IMMUTABILITY: Only Draft invoices can be updated
    /// </summary>
    public class UpdateInvoiceDto
    {
        /// <summary>
        /// Invoice notes (only field that can be updated on Draft invoices)
        /// </summary>
        public string? Notes { get; set; }
    }
}
