using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Invoice.Dtos
{
   public class UpdateInvoiceDto
    {

        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime IssueDate { get; set; }
        public string Notes { get; set; }
        public int GuestId { get; set; }
        public int? TransferId { get; set; }
        public int? CityTourId { get; set; }
        public int? YachtTourId { get; set; }


    }
}
