using FluentValidation;
using GuestFlow.Api.Models.PersonnelModels;
using GuestFlow.Domain.Entities.Enum;

namespace GuestFlow.Api.Validators
{
    public class ChangeRoleRequestValidator : AbstractValidator<ChangeRoleRequest>
    {
        public ChangeRoleRequestValidator()
        {
            RuleFor(x => x.UserType)
                .IsInEnum().WithMessage("Geçerli bir kullanıcı tipi seçiniz.");
        }
    }
}

