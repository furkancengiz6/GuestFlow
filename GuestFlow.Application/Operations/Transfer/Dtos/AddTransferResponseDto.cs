namespace GuestFlow.Application.Operations.Transfer.Dtos
{
    /// <summary>
    /// Transfer ekleme işlemi sonrası dönen response DTO
    /// </summary>
    public class AddTransferResponseDto
    {
        public int TransferId { get; set; }
        public int? InvoiceId { get; set; }
        public string? InvoicePdfUrl { get; set; }
    }
}

