namespace GuestFlow.Domain.DataProtection
{
    public interface IDataProtection
    {
        string Protect(string text);
        string Unprotect(string protectedText);
    }
}

