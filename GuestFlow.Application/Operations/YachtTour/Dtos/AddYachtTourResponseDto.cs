namespace GuestFlow.Application.Operations.YachtTour.Dtos
{
    /// <summary>
    /// Yat turu ekleme işlemi sonrası dönen response DTO
    /// </summary>
    public class AddYachtTourResponseDto
    {
        public int YachtTourId { get; set; }
        public int? InvoiceId { get; set; }
        public string? InvoicePdfUrl { get; set; }
    }
}

