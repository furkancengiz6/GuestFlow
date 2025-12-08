using FluentValidation;
using GuestFlow.Api.Models.YachtTourModels;

namespace GuestFlow.Api.Validators
{
    public class AddYachtTourRequestValidator : AbstractValidator<AddYachtTourRequest>
    {
        public AddYachtTourRequestValidator()
        {
            RuleFor(x => x.TourDate)
                .NotEmpty().WithMessage("Tur tarihi gereklidir.")
                .Must(BeFutureOrToday).WithMessage("Tur tarihi bugünden önceki bir tarih olamaz.");

            RuleFor(x => x.NumberOfPeople)
                .GreaterThan(0).WithMessage("Kişi sayısı 0'dan büyük olmalıdır.")
                .LessThanOrEqualTo(100).WithMessage("Kişi sayısı 100'den fazla olamaz.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.")
                .LessThanOrEqualTo(1000000).WithMessage("Fiyat çok yüksek.");

            RuleFor(x => x.YachtName)
                .NotEmpty().WithMessage("Yat adı gereklidir.")
                .MaximumLength(100).WithMessage("Yat adı en fazla 100 karakter olabilir.");

            RuleFor(x => x.OwnerGuestId)
                .GreaterThan(0).WithMessage("Geçerli bir misafir ID'si gereklidir.");

            RuleFor(x => x.PersonnelId)
                .GreaterThan(0).WithMessage("Geçerli bir personel ID'si gereklidir.");

            RuleFor(x => x.CityId)
                .GreaterThan(0).WithMessage("Geçerli bir şehir ID'si gereklidir.");

            RuleFor(x => x.SpecialRequest)
                .MaximumLength(500).WithMessage("Özel istek en fazla 500 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.SpecialRequest));

            RuleFor(x => x.DiscountPercentage)
                .InclusiveBetween(0, 100).WithMessage("İndirim yüzdesi 0 ile 100 arasında olmalıdır.")
                .When(x => x.DiscountPercentage.HasValue);

            RuleFor(x => x.InvoiceDescription)
                .MaximumLength(500).WithMessage("Fatura açıklaması en fazla 500 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.InvoiceDescription));

            RuleFor(x => x.Currency)
                .Must(BeValidCurrencyCode).WithMessage("Geçerli bir para birimi kodu giriniz (TRY, USD, EUR, GBP, RUB).")
                .When(x => !string.IsNullOrEmpty(x.Currency));
        }

        private bool BeValidCurrencyCode(string? currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
                return true; // Opsiyonel alan, boş olabilir

            var validCodes = new[] { "TRY", "USD", "EUR", "GBP", "RUB" };
            return validCodes.Contains(currencyCode.ToUpper());
        }

        private bool BeFutureOrToday(DateTime date)
        {
            return date.Date >= DateTime.UtcNow.Date;
        }
    }
}

