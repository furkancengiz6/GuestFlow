using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Transfer.Dtos
{
    public class UpdateTransferDto
    {
        public int Id { get; set; }
        public DateTime TransferDate { get; set; }
        public string PickupAddress { get; set; }
        public string DropoffAddress { get; set; }
        public decimal Price { get; set; }
        public int GuestId { get; set; }
        public int PersonnelId { get; set; }
        public int AirportId { get; set; }
        public int VehicleId { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
        public bool IsFromAirport { get; set; }
        public int PickupCityId { get; set; } // Yeni eklenen alan
        public int DropoffCityId { get; set; } // Yeni eklenen alan
    }
}
