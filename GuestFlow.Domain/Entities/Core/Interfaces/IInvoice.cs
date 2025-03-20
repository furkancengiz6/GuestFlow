using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core.Interfaces
{
    public interface IInvoice
    {
        int InvoiceNumber { get; set; }
        DateTime IssueDate { get; set; }
        decimal TotalAmount { get; set; }
        string Currency {  get; set; }
        string Notes { get; set; }
        string PdfUrl { get; set; }

    }
}
