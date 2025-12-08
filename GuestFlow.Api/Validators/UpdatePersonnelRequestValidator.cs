using FluentValidation;
using GuestFlow.Api.Models.PersonnelModels;
using GuestFlow.Domain.Entities.Enum;

namespace GuestFlow.Api.Validators
{
    public class UpdatePersonnelRequestValidator : AbstractValidator<UpdatePersonnelRequest>
    {
        public UpdatePersonnelRequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Geçerli bir personel ID'si gereklidir.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Ad Soyad gereklidir.")
                .Length(5, 200).WithMessage("Ad Soyad 5 ile 200 karakter arasında olmalıdır.")
                .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$").WithMessage("Ad Soyad sadece harf içerebilir.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta adresi gereklidir.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
                .MaximumLength(255).WithMessage("E-posta adresi en fazla 255 karakter olabilir.");

            RuleFor(x => x.UserType)
                .IsInEnum().WithMessage("Geçerli bir kullanıcı tipi seçiniz.")
                .When(x => x.UserType.HasValue);

            RuleFor(x => x.NewPassword)
                .MinimumLength(8).WithMessage("Yeni şifre en az 8 karakter olmalıdır.")
                .Matches(@"[A-Z]").WithMessage("Yeni şifre en az bir büyük harf içermelidir.")
                .Matches(@"[a-z]").WithMessage("Yeni şifre en az bir küçük harf içermelidir.")
                .Matches(@"[0-9]").WithMessage("Yeni şifre en az bir rakam içermelidir.")
                .Matches(@"[!@#$%^&*(),.?\"":{}|<>]").WithMessage("Yeni şifre en az bir özel karakter içermelidir.")
                .When(x => !string.IsNullOrEmpty(x.NewPassword));
        }
    }
}

