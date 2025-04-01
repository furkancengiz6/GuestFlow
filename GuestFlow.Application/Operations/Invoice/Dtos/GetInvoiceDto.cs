// GuestFlow.Application/Operations/Invoice/Dtos/GetInvoiceDto.cs
namespace GuestFlow.Application.Operations.Invoice.Dtos
{
    public class GetInvoiceDto
    {
        public int Id { get; set; }
        public int InvoiceNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime IssueDate { get; set; }
        public string Currency { get; set; }
        public string Notes { get; set; }
        public string PdfUrl { get; set; }
        public int GuestId { get; set; }
        public int? PersonnelId { get; set; }
        public int? TransferId { get; set; }
        public int? CityTourId { get; set; }
        public int? YachtTourId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}