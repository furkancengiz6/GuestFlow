using FluentValidation;
using GuestFlow.Api.Models;

namespace GuestFlow.Api.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Ad Soyad gereklidir.")
                .Length(5, 200).WithMessage("Ad Soyad 5 ile 200 karakter arasında olmalıdır.")
                .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$").WithMessage("Ad Soyad sadece harf içerebilir.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta adresi gereklidir.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
                .MaximumLength(255).WithMessage("E-posta adresi en fazla 255 karakter olabilir.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre gereklidir.")
                .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
                .Matches(@"[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
                .Matches(@"[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
                .Matches(@"[0-9]").WithMessage("Şifre en az bir rakam içermelidir.")
                .Matches(@"[!@#$%^&*(),.?\"":{}|<>]").WithMessage("Şifre en az bir özel karakter içermelidir.");
        }
    }
}

