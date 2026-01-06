using FluentValidation;
using GuestFlow.Api.Models;
using System.Text.RegularExpressions;

namespace GuestFlow.Api.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Ad Soyad gereklidir.")
                .Length(2, 200).WithMessage("Ad Soyad 2 ile 200 karakter arasında olmalıdır.")
                .Must(BeSafeName).WithMessage("Ad Soyad geçersiz karakterler içeriyor.")
                .Must(NotContainInjection).WithMessage("Geçersiz giriş tespit edildi.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta adresi gereklidir.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
                .MaximumLength(254).WithMessage("E-posta adresi en fazla 254 karakter olabilir.")
                .Must(BeSafeEmail).WithMessage("E-posta adresi geçersiz karakterler içeriyor.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre gereklidir.")
                .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
                .MaximumLength(128).WithMessage("Şifre çok uzun.")
                .Matches(@"[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
                .Matches(@"[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
                .Matches(@"[0-9]").WithMessage("Şifre en az bir rakam içermelidir.")
                .Matches(@"[!@#$%^&*()_+\-=[\]{};':""\\|,.<>/?]").WithMessage("Şifre en az bir özel karakter içermelidir.")
                .Must(NotContainCommonPasswords).WithMessage("Bu şifre çok yaygın, lütfen daha güçlü bir şifre seçin.");
        }

        private bool BeSafeName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return true;

            // Allow Turkish characters and basic punctuation
            var safePattern = @"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s\-\.']+$";
            return Regex.IsMatch(name, safePattern);
        }

        private bool BeSafeEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return true;

            // SECURITY: Prevent XSS and injection in email
            var dangerousPatterns = new[] {
                @"<script", @"javascript:", @"vbscript:",
                @"on\w+\s*=", @"style\s*=",
                @"[^\x20-\x7E]", // Non-printable ASCII
                @"['\""]", // Quotes that might be used for SQL injection
                @"(--|#|/\*|\*/)", // SQL comments
                @"(union|select|insert|update|delete|drop|create|alter|exec)\s" // SQL keywords
            };

            foreach (var pattern in dangerousPatterns)
            {
                if (Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase))
                    return false;
            }

            return true;
        }

        private bool NotContainInjection(string input)
        {
            if (string.IsNullOrEmpty(input))
                return true;

            // Basic injection detection
            var injectionPatterns = new[] {
                @"<script", @"javascript:", @"vbscript:",
                @"on\w+\s*=", @"style\s*=",
                @"('|""|\-\-|#|/\*|\*/)", // SQL injection basics
                @"(union|select|insert|update|delete|drop|create|alter|exec)\s" // SQL keywords
            };

            foreach (var pattern in injectionPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                    return false;
            }

            return true;
        }

        private bool NotContainCommonPasswords(string password)
        {
            if (string.IsNullOrEmpty(password))
                return true;

            // SECURITY: Block extremely common passwords
            var commonPasswords = new[] {
                "password", "123456", "123456789", "qwerty",
                "abc123", "password123", "admin", "letmein",
                "welcome", "monkey", "dragon", "password1"
            };

            return !commonPasswords.Contains(password.ToLowerInvariant());
        }
    }
}

