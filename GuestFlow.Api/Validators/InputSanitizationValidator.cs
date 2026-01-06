using FluentValidation;
using System.Text.RegularExpressions;

namespace GuestFlow.Api.Validators
{
    /// <summary>
    /// Base validator with common input sanitization rules for security
    /// </summary>
    public static class InputSanitizationValidator
    {
        /// <summary>
        /// Validates input against XSS and basic injection attacks
        /// </summary>
        public static IRuleBuilderOptions<T, string> SafeInput<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            string fieldName = "Input",
            int maxLength = 1000)
        {
            return ruleBuilder
                .MaximumLength(maxLength)
                .WithMessage($"{fieldName} çok uzun.")
                .Must(input => !ContainsXss(input))
                .WithMessage($"{fieldName} güvenli olmayan içerik içeriyor.")
                .Must(input => !ContainsSqlInjection(input))
                .WithMessage($"{fieldName} geçersiz karakterler içeriyor.");
        }

        /// <summary>
        /// Validates email against injection and XSS attacks
        /// </summary>
        public static IRuleBuilderOptions<T, string> SafeEmail<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .EmailAddress()
                .WithMessage("Geçerli bir e-posta adresi giriniz.")
                .MaximumLength(254)
                .WithMessage("E-posta adresi çok uzun.")
                .Must(email => !ContainsEmailAttacks(email))
                .WithMessage("E-posta adresi geçersiz karakterler içeriyor.");
        }

        /// <summary>
        /// Validates name fields against XSS and injection
        /// </summary>
        public static IRuleBuilderOptions<T, string> SafeName<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            int minLength = 2,
            int maxLength = 200)
        {
            return ruleBuilder
                .Length(minLength, maxLength)
                .WithMessage($"İsim {minLength} ile {maxLength} karakter arasında olmalıdır.")
                .Must(name => !ContainsXss(name))
                .WithMessage("İsim güvenli olmayan karakterler içeriyor.")
                .Must(name => !ContainsInjectionInName(name))
                .WithMessage("İsim geçersiz karakterler içeriyor.");
        }

        private static bool ContainsXss(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var xssPatterns = new[] {
                @"<script", @"</script>", @"javascript:", @"vbscript:",
                @"on\w+\s*=", @"style\s*=", @"<iframe", @"<object",
                @"<embed", @"<form", @"<input", @"<meta",
                @"expression\s*\(", @"vbscript:", @"data:text",
                @"&#", @"%3c", @"%3e" // Encoded brackets
            };

            var lowerInput = input.ToLowerInvariant();
            return xssPatterns.Any(pattern => lowerInput.Contains(pattern.ToLowerInvariant()));
        }

        private static bool ContainsSqlInjection(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var sqlPatterns = new[] {
                @"('|""|--|#|/\*|\*/)", // SQL delimiters and comments
                @"(\b(union|select|insert|update|delete|drop|create|alter|exec|execute)\b)", // SQL keywords
                @"(\b(and|or|not|like|between|in|exists|having|group|order|limit)\b.*(=|>|<|like))", // SQL operators
                @"(;|\|)", // Command separators
                @"(\b(xp_|sp_|sys|information_schema|mysql)\.)", // System objects
                @"(0x[0-9a-f]+)", // Hex encoded data
                @"(char\s*\()", @"convert\s*\(", @"cast\s*\(" // Type conversion functions
            };

            foreach (var pattern in sqlPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline))
                    return true;
            }

            return false;
        }

        private static bool ContainsEmailAttacks(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var attackPatterns = new[] {
                @"<script", @"javascript:", @"vbscript:", @"data:",
                @"('|""|\r|\n|\t)", // Control characters
                @"(\.\.)+", @"(\\\\+)", // Path traversal
                @"(0x[0-9a-f]+)", @"(&#|%[0-9a-f]{2})", // Encoded characters
                @"(\b(union|select|insert|update|delete)\b)", // SQL keywords
                @"(\.\./|\.\.\\)", // Directory traversal
                @"(\|)", // Pipe for command injection
                @"(\$\{.*\})" // Template injection
            };

            foreach (var pattern in attackPatterns)
            {
                if (Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool ContainsInjectionInName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var injectionPatterns = new[] {
                @"<script", @"javascript:", @"vbscript:",
                @"('|""|--|#|/\*|\*/)", // SQL injection
                @"(\b(union|select|insert|update|delete)\b)", // SQL keywords
                @"(\.\./|\.\.\\)", // Path traversal
                @"(\$\{.*\})", // Template injection
                @"[^\w\sğüşıöçĞÜŞİÖÇ\-\.\s']" // Only allow safe characters for names
            };

            foreach (var pattern in injectionPatterns)
            {
                if (Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
