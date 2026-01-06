namespace GuestFlow.Application.Operations.CityTour.Dtos
{
    /// <summary>
    /// Şehir turu ekleme işlemi sonrası dönen response DTO
    /// </summary>
    public class AddCityTourResponseDto
    {
        public int CityTourId { get; set; }
        public int? InvoiceId { get; set; }
        public string? InvoicePdfUrl { get; set; }
    }
}

