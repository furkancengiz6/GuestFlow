using GuestFlow.Domain.Entities.Enum;

namespace GuestFlow.Api.Models
{
    public class UserInfoResponse
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserType UserType { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}

