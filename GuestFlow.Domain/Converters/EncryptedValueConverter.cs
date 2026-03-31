using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using GuestFlow.Domain.DataProtection;
using System;

namespace GuestFlow.Domain.Converters
{
    /// <summary>
    /// EF Core Value Converter that encrypts data before saving to DB 
    /// and decrypts it when reading from DB.
    /// </summary>
    public class EncryptedValueConverter : ValueConverter<string?, string?>
    {
        public EncryptedValueConverter() : base(
            v => Protect(v),
            v => Unprotect(v))
        {
        }

        private static string? Protect(string? value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return DataProtectionBridge.GetService().Protect(value);
        }

        private static string? Unprotect(string? value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            try
            {
                return DataProtectionBridge.GetService().Unprotect(value);
            }
            catch (Exception)
            {
                // In case of decryption error (e.g. data was not encrypted), 
                // we might want to return the original value or log it.
                // For a robust system, we should have a way to identify encrypted data.
                return value;
            }
        }
    }
}
