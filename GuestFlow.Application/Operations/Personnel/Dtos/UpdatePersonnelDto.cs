namespace GuestFlow.Application.Operations.Personnel.Dtos
{
    public class UpdatePersonnelDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Domain.Entities.Enum.UserType? UserType { get; set; }
        public string? NewPassword { get; set; } // Şifre değiştirmek için (opsiyonel)
    }
}

