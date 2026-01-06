// GuestFlow.Application/Operations/Invoice/Dtos/GetInvoiceDto.cs
using GuestFlow.Domain.Entities.Core;

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
        public InvoiceStatus Status { get; set; }
        public bool IsPdfGenerated { get; set; }
        public DateTime? PdfGeneratedDate { get; set; }
        public int? LockedByPersonnelId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<InvoiceItemDto> InvoiceItems { get; set; } = new List<InvoiceItemDto>();
    }

    public class InvoiceItemDto
    {
        public int Id { get; set; }
        public string ServiceType { get; set; }
        public int ServiceId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}