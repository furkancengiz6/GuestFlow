using FluentValidation;
using GuestFlow.Api.Models;
using System.Text.RegularExpressions;

namespace GuestFlow.Api.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta adresi gereklidir.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
                .MaximumLength(254).WithMessage("E-posta adresi çok uzun.")
                .Must(BeSafeEmail).WithMessage("Geçersiz karakterler içeriyor.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre gereklidir.")
                .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
                .MaximumLength(128).WithMessage("Şifre çok uzun.")
                .Must(ContainValidCharacters).WithMessage("Şifre geçersiz karakterler içeriyor.");
        }

        private bool BeSafeEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return true;

            // SECURITY: Prevent email-based attacks
            var dangerousPatterns = new[] {
                @"<script", @"javascript:", @"vbscript:",
                @"on\w+\s*=", @"style\s*=",
                @"[^\x20-\x7E]" // Non-ASCII characters that might be used for attacks
            };

            foreach (var pattern in dangerousPatterns)
            {
                if (Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase))
                    return false;
            }

            return true;
        }

        private bool ContainValidCharacters(string password)
        {
            if (string.IsNullOrEmpty(password))
                return true;

            // SECURITY: Allow only safe characters, prevent injection attacks
            var allowedPattern = @"^[a-zA-Z0-9!@#$%^&*()_+\-=[\]{};':""\\|,.<>/?`~]+$";
            return Regex.IsMatch(password, allowedPattern);
        }
    }
}

