using GuestFlow.Domain.DataProtection;
using System;

namespace GuestFlow.Domain.DataProtection
{
    /// <summary>
    /// Static bridge to allow access to IDataProtection from EF Core Value Converters.
    /// This is initialized at application startup.
    /// </summary>
    public static class DataProtectionBridge
    {
        private static IDataProtection? _dataProtection;

        public static void Initialize(IDataProtection dataProtection)
        {
            _dataProtection = dataProtection ?? throw new ArgumentNullException(nameof(dataProtection));
        }

        public static IDataProtection GetService()
        {
            return _dataProtection ?? throw new InvalidOperationException("DataProtectionBridge has not been initialized. Call Initialize() during startup.");
        }
    }
}
