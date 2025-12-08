namespace GuestFlow.Application.Operations.Password
{
    public interface IPasswordService
    {
        /// <summary>
        /// Şifre güçlülüğünü kontrol eder
        /// </summary>
        PasswordValidationResult ValidatePassword(string password);

        /// <summary>
        /// Şifre güçlülük skorunu hesaplar (0-100)
        /// </summary>
        int CalculatePasswordStrength(string password);
    }

    public class PasswordValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
        public int StrengthScore { get; set; }
    }
}

