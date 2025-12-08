using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.TransferModel
{
    public class UpdateTransferStatusRequest
    {
        [Required(ErrorMessage = "Durum zorunludur.")]
        public string Status { get; set; } = string.Empty;
    }
}

