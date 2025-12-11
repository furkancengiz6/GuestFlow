using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.GuestModels
{
    
   public class AddGuestRequest
   {
     [Required]   
     [Length(5, 100)] 
     public string FullName { get; set; } = string.Empty;

     
     public string? Email { get; set; }

      
        public string? PhoneNumber { get; set; }

        [Required]
        [Length(2, 100)] 
     public string Nationality { get; set; } = string.Empty;
        [Required]
        public bool IsSpecialGuest { get; set; }
    }
}

