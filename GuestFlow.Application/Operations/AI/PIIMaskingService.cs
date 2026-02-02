using System.Text.RegularExpressions;

namespace GuestFlow.Application.Operations.AI
{
    /// <summary>
    /// Kişisel verileri (PII) AI servislerine gönderilmeden önce maskeleyen servis
    /// </summary>
    public interface IPIIMaskingService
    {
        string MaskPII(string text);
    }

    public class PIIMaskingService : IPIIMaskingService
    {
        private static readonly Regex EmailRegex = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex PhoneRegex = new Regex(@"(\+?\d{1,3}[- ]?)?\d{10}", RegexOptions.Compiled);

        public string MaskPII(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var maskedText = text;

            // Mask emails
            maskedText = EmailRegex.Replace(maskedText, "[EMAIL_MASKED]");

            // Mask phones
            maskedText = PhoneRegex.Replace(maskedText, "[PHONE_MASKED]");

            return maskedText;
        }
    }
}
