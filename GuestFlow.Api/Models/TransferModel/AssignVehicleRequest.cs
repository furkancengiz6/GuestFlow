using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.TransferModel
{
    public class AssignVehicleRequest
    {
        [Required(ErrorMessage = "Araç ID zorunludur.")]
        public int VehicleId { get; set; }
    }
}

