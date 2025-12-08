using FluentValidation;
using GuestFlow.Api.Models;

namespace GuestFlow.Api.Validators
{
    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Mevcut şifre gereklidir.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Yeni şifre gereklidir.")
                .MinimumLength(8).WithMessage("Yeni şifre en az 8 karakter olmalıdır.")
                .Matches(@"[A-Z]").WithMessage("Yeni şifre en az bir büyük harf içermelidir.")
                .Matches(@"[a-z]").WithMessage("Yeni şifre en az bir küçük harf içermelidir.")
                .Matches(@"[0-9]").WithMessage("Yeni şifre en az bir rakam içermelidir.")
                .Matches(@"[!@#$%^&*(),.?\"":{}|<>]").WithMessage("Yeni şifre en az bir özel karakter içermelidir.")
                .NotEqual(x => x.CurrentPassword).WithMessage("Yeni şifre mevcut şifre ile aynı olamaz.");
        }
    }
}

