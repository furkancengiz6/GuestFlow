using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GuestFlow.Application.Operations.Password
{
    public class PasswordService : IPasswordService
    {
        private readonly int _minLength = 8;
        private readonly int _maxLength = 128;
        private readonly bool _requireUppercase = true;
        private readonly bool _requireLowercase = true;
        private readonly bool _requireDigit = true;
        private readonly bool _requireSpecialChar = true;

        public PasswordValidationResult ValidatePassword(string password)
        {
            var result = new PasswordValidationResult
            {
                Errors = new List<string>()
            };

            if (string.IsNullOrWhiteSpace(password))
            {
                result.Errors.Add("Şifre boş olamaz.");
                result.IsValid = false;
                result.Message = "Şifre gereksinimleri karşılanmıyor.";
                return result;
            }

            // Minimum uzunluk kontrolü
            if (password.Length < _minLength)
            {
                result.Errors.Add($"Şifre en az {_minLength} karakter olmalıdır.");
            }

            // Maksimum uzunluk kontrolü
            if (password.Length > _maxLength)
            {
                result.Errors.Add($"Şifre en fazla {_maxLength} karakter olabilir.");
            }

            // Büyük harf kontrolü
            if (_requireUppercase && !password.Any(char.IsUpper))
            {
                result.Errors.Add("Şifre en az bir büyük harf içermelidir.");
            }

            // Küçük harf kontrolü
            if (_requireLowercase && !password.Any(char.IsLower))
            {
                result.Errors.Add("Şifre en az bir küçük harf içermelidir.");
            }

            // Rakam kontrolü
            if (_requireDigit && !password.Any(char.IsDigit))
            {
                result.Errors.Add("Şifre en az bir rakam içermelidir.");
            }

            // Özel karakter kontrolü
            if (_requireSpecialChar)
            {
                var specialCharPattern = new Regex(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]");
                if (!specialCharPattern.IsMatch(password))
                {
                    result.Errors.Add("Şifre en az bir özel karakter içermelidir (!@#$%^&* vb.).");
                }
            }

            // Yaygın şifre kontrolü
            var commonPasswords = new[] { "password", "12345678", "qwerty", "abc123", "password123", "admin123" };
            if (commonPasswords.Any(cp => password.ToLower().Contains(cp)))
            {
                result.Errors.Add("Şifre çok yaygın bir şifre içeriyor. Lütfen daha güvenli bir şifre seçin.");
            }

            // Ardışık karakter kontrolü
            if (HasConsecutiveChars(password, 3))
            {
                result.Errors.Add("Şifre ardışık karakterler içermemelidir (ör: abc, 123).");
            }

            // Tekrarlayan karakter kontrolü
            if (HasRepeatingChars(password, 3))
            {
                result.Errors.Add("Şifre aynı karakterin 3 veya daha fazla tekrarını içermemelidir.");
            }

            result.IsValid = result.Errors.Count == 0;
            result.Message = result.IsValid
                ? "Şifre gereksinimleri karşılanıyor."
                : "Şifre gereksinimleri karşılanmıyor.";
            result.StrengthScore = CalculatePasswordStrength(password);

            return result;
        }

        public int CalculatePasswordStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return 0;

            int score = 0;

            // Uzunluk skoru (0-25 puan)
            if (password.Length >= _minLength) score += 10;
            if (password.Length >= 12) score += 10;
            if (password.Length >= 16) score += 5;

            // Karakter çeşitliliği (0-40 puan)
            if (password.Any(char.IsUpper)) score += 10;
            if (password.Any(char.IsLower)) score += 10;
            if (password.Any(char.IsDigit)) score += 10;
            if (password.Any(ch => !char.IsLetterOrDigit(ch))) score += 10;

            // Karmaşıklık (0-20 puan)
            var uniqueChars = password.Distinct().Count();
            if (uniqueChars >= password.Length * 0.6) score += 10;
            if (uniqueChars >= password.Length * 0.8) score += 10;

            // Güvenlik (0-15 puan)
            if (!HasConsecutiveChars(password, 3)) score += 5;
            if (!HasRepeatingChars(password, 3)) score += 5;
            if (!HasCommonPatterns(password)) score += 5;

            return Math.Min(100, score);
        }

        private bool HasConsecutiveChars(string password, int count)
        {
            for (int i = 0; i <= password.Length - count; i++)
            {
                var substring = password.Substring(i, count);
                if (IsConsecutive(substring))
                    return true;
            }
            return false;
        }

        private bool IsConsecutive(string str)
        {
            if (str.Length < 2) return false;

            bool isAscending = true;
            bool isDescending = true;

            for (int i = 1; i < str.Length; i++)
            {
                int diff = str[i] - str[i - 1];
                if (diff != 1) isAscending = false;
                if (diff != -1) isDescending = false;
            }

            return isAscending || isDescending;
        }

        private bool HasRepeatingChars(string password, int count)
        {
            for (int i = 0; i <= password.Length - count; i++)
            {
                var substring = password.Substring(i, count);
                if (substring.Distinct().Count() == 1)
                    return true;
            }
            return false;
        }

        private bool HasCommonPatterns(string password)
        {
            var commonPatterns = new[]
            {
                "123", "321", "abc", "cba", "qwe", "asd", "zxc"
            };

            var lowerPassword = password.ToLower();
            return commonPatterns.Any(pattern => lowerPassword.Contains(pattern));
        }
    }
}

