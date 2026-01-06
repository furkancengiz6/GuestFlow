using System;
using System.Text.RegularExpressions;
using GuestFlow.Application.Types;

namespace GuestFlow.Application.Operations.Validation
{
    /// <summary>
    /// Input validation service for security hardening
    /// Prevents common injection attacks and validates input patterns
    /// </summary>
    public class InputValidationService
    {
        private static readonly string[] SqlKeywords = {
            "select", "insert", "update", "delete", "drop", "create", "alter",
            "exec", "execute", "union", "join", "where", "having", "group by",
            "order by", "limit", "script", "javascript", "vbscript", "onload",
            "onerror", "alert", "eval", "function", "script"
        };

        private static readonly Regex SqlInjectionRegex = new Regex(
            @"(\b(" + string.Join("|", SqlKeywords) + @")\b)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex XssRegex = new Regex(
            @"<script[^>]*>.*?</script>|<.*?javascript:.*?>|<.*?vbscript:.*?>|<.*?onload=.*?>|<.*?onerror=.*?>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex EmailRegex = new Regex(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled);

        private static readonly Regex PhoneRegex = new Regex(
            @"^[\+]?[1-9][\d]{0,15}$",
            RegexOptions.Compiled);

        /// <summary>
        /// Validates input for potential security threats
        /// </summary>
        public ServiceMessage ValidateSecureInput(string input, string fieldName, int maxLength = 1000)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new ServiceMessage { IsSuccess = true, Message = "Input is valid" };
            }

            if (input.Length > maxLength)
            {
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"{fieldName} cannot exceed {maxLength} characters"
                };
            }

            // Check for SQL injection patterns
            if (SqlInjectionRegex.IsMatch(input))
            {
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"{fieldName} contains potentially dangerous content"
                };
            }

            // Check for XSS patterns
            if (XssRegex.IsMatch(input))
            {
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"{fieldName} contains potentially dangerous scripts"
                };
            }

            // Check for null bytes (common in file upload attacks)
            if (input.Contains('\0'))
            {
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"{fieldName} contains invalid characters"
                };
            }

            return new ServiceMessage { IsSuccess = true, Message = "Input is valid" };
        }

        /// <summary>
        /// Validates email format and security
        /// </summary>
        public ServiceMessage ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = "Email is required"
                };
            }

            if (!EmailRegex.IsMatch(email))
            {
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = "Invalid email format"
                };
            }

            // Additional security checks
            var securityCheck = ValidateSecureInput(email, "Email", 254);
            if (!securityCheck.IsSuccess)
            {
                return securityCheck;
            }

            return new ServiceMessage { IsSuccess = true, Message = "Email is valid" };
        }

        /// <summary>
        /// Validates phone number format
        /// </summary>
        public ServiceMessage ValidatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return new ServiceMessage { IsSuccess = true, Message = "Phone is optional" };
            }

            // Remove common separators for validation
            var cleanPhone = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");

            if (!PhoneRegex.IsMatch(cleanPhone))
            {
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = "Invalid phone number format"
                };
            }

            return new ServiceMessage { IsSuccess = true, Message = "Phone number is valid" };
        }

        /// <summary>
        /// Validates name fields (no special characters that could be used for injection)
        /// </summary>
        public ServiceMessage ValidateName(string name, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"{fieldName} is required"
                };
            }

            // Allow only letters, spaces, hyphens, and apostrophes
            if (!Regex.IsMatch(name, @"^[a-zA-Z\s\-']+$"))
            {
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"{fieldName} can only contain letters, spaces, hyphens, and apostrophes"
                };
            }

            var securityCheck = ValidateSecureInput(name, fieldName, 100);
            if (!securityCheck.IsSuccess)
            {
                return securityCheck;
            }

            return new ServiceMessage { IsSuccess = true, Message = $"{fieldName} is valid" };
        }

        /// <summary>
        /// Validates address fields
        /// </summary>
        public ServiceMessage ValidateAddress(string address, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"{fieldName} is required"
                };
            }

            var securityCheck = ValidateSecureInput(address, fieldName, 500);
            if (!securityCheck.IsSuccess)
            {
                return securityCheck;
            }

            return new ServiceMessage { IsSuccess = true, Message = $"{fieldName} is valid" };
        }

        /// <summary>
        /// Validates monetary amounts
        /// </summary>
        public ServiceMessage ValidateAmount(decimal amount, string fieldName)
        {
            if (amount < 0)
            {
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"{fieldName} cannot be negative"
                };
            }

            if (amount > 999999.99m)
            {
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"{fieldName} cannot exceed 999,999.99"
                };
            }

            return new ServiceMessage { IsSuccess = true, Message = $"{fieldName} is valid" };
        }

        /// <summary>
        /// Validates date ranges for business logic
        /// </summary>
        public ServiceMessage ValidateDateRange(DateTime startDate, DateTime endDate, string context)
        {
            if (startDate > endDate)
            {
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"{context}: Start date cannot be after end date"
                };
            }

            if (startDate < DateTime.Now.AddYears(-1))
            {
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"{context}: Start date cannot be more than 1 year in the past"
                };
            }

            if (endDate > DateTime.Now.AddYears(2))
            {
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"{context}: End date cannot be more than 2 years in the future"
                };
            }

            return new ServiceMessage { IsSuccess = true, Message = $"{context} date range is valid" };
        }
    }
}