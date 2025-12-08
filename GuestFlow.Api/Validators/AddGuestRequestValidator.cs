using FluentValidation;
using GuestFlow.Api.Models.GuestModels;

namespace GuestFlow.Api.Validators
{
    public class AddGuestRequestValidator : AbstractValidator<AddGuestRequest>
    {
        public AddGuestRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Ad Soyad gereklidir.")
                .Length(5, 100).WithMessage("Ad Soyad 5 ile 100 karakter arasında olmalıdır.")
                .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$").WithMessage("Ad Soyad sadece harf içerebilir.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
                .When(x => !string.IsNullOrEmpty(x.Email))
                .MaximumLength(255).WithMessage("E-posta adresi en fazla 255 karakter olabilir.");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Geçerli bir telefon numarası giriniz.")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .MaximumLength(20).WithMessage("Telefon numarası en fazla 20 karakter olabilir.");

            RuleFor(x => x.Nationality)
                .NotEmpty().WithMessage("Uyruk gereklidir.")
                .Length(2, 100).WithMessage("Uyruk 2 ile 100 karakter arasında olmalıdır.")
                .Matches(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$").WithMessage("Uyruk sadece harf içerebilir.");
        }
    }
}

