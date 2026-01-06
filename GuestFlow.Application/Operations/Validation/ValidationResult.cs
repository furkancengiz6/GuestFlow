using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Validation
{
    /// <summary>
    /// Business rules validation result
    /// İş kuralları validasyon sonucu
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
        public string FieldName { get; set; } = string.Empty;

        // Backward compatibility
        public string ErrorMessage
        {
            get => string.Join("; ", ErrorMessages);
            set
            {
                ErrorMessages.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    ErrorMessages.Add(value);
                }
            }
        }
    }
}