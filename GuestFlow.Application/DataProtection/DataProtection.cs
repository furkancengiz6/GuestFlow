using GuestFlow.Domain.DataProtection;
using Microsoft.AspNetCore.DataProtection;

namespace GuestFlow.Application.DataProtection
{
    public class DataProtection : IDataProtection
    {
        private readonly IDataProtector _dataProtector;

        public DataProtection(IDataProtectionProvider provider) 
        {
            _dataProtector = provider.CreateProtector("GuestFlow-security-v1");
        }


        public string Protect(string text)
        {
            return _dataProtector.Protect(text);
        }

        public string Unprotect(string protectedText)
        {
            return _dataProtector.Unprotect(protectedText);
        }
    }
}
