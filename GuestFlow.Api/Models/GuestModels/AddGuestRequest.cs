using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.GuestModels
{
    
   public class AddGuestRequest
   {
     [Required]   
     [Length(5, 100)] 
     public string FullName { get; set; }

     
     public string? Email { get; set; }

      
        public string? PhoneNumber { get; set; }

        [Required]
        [Length(2, 100)] 
     public string Nationality { get; set; }
        [Required]
        public bool IsSpecialGuest { get; set; }
    }
}

