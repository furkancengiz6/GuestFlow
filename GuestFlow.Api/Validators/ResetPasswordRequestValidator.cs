using FluentValidation;
using GuestFlow.Api.Models;

namespace GuestFlow.Api.Validators
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token gereklidir.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Yeni şifre gereklidir.")
                .MinimumLength(8).WithMessage("Yeni şifre en az 8 karakter olmalıdır.")
                .Matches(@"[A-Z]").WithMessage("Yeni şifre en az bir büyük harf içermelidir.")
                .Matches(@"[a-z]").WithMessage("Yeni şifre en az bir küçük harf içermelidir.")
                .Matches(@"[0-9]").WithMessage("Yeni şifre en az bir rakam içermelidir.")
                .Matches(@"[!@#$%^&*(),.?\"":{}|<>]").WithMessage("Yeni şifre en az bir özel karakter içermelidir.");
        }
    }
}

