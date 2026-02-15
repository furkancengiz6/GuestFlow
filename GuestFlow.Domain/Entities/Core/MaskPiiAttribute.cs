using System;

namespace GuestFlow.Domain.Entities.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MaskPiiAttribute : Attribute
    {
        public PiiType Type { get; }

        public MaskPiiAttribute(PiiType type)
        {
            Type = type;
        }
    }

    public enum PiiType
    {
        Email,
        Phone,
        Passport,
        IdentityNumber,
        Address,
        CreditCard
    }
}
