using FluentValidation;
using GuestFlow.Api.Models;

namespace GuestFlow.Api.Validators
{
    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta adresi gereklidir.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
                .MaximumLength(255).WithMessage("E-posta adresi en fazla 255 karakter olabilir.");
        }
    }
}

