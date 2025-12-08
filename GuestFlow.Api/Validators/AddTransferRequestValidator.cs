using FluentValidation;
using GuestFlow.Api.Models.TransferModel;

namespace GuestFlow.Api.Validators
{
    public class AddTransferRequestValidator : AbstractValidator<AddTransferRequest>
    {
        public AddTransferRequestValidator()
        {
            RuleFor(x => x.TransferDate)
                .NotEmpty().WithMessage("Transfer tarihi gereklidir.")
                .Must(BeFutureOrToday).WithMessage("Transfer tarihi bugünden önceki bir tarih olamaz.");

            RuleFor(x => x.PickupAddress)
                .NotEmpty().WithMessage("Alış adresi gereklidir.")
                .MaximumLength(200).WithMessage("Alış adresi en fazla 200 karakter olabilir.");

            RuleFor(x => x.DropoffAddress)
                .NotEmpty().WithMessage("Bırakış adresi gereklidir.")
                .MaximumLength(200).WithMessage("Bırakış adresi en fazla 200 karakter olabilir.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.")
                .LessThanOrEqualTo(1000000).WithMessage("Fiyat çok yüksek.");

            RuleFor(x => x.GuestId)
                .GreaterThan(0).WithMessage("Geçerli bir misafir ID'si gereklidir.");

            RuleFor(x => x.PersonnelId)
                .GreaterThan(0).WithMessage("Geçerli bir personel ID'si gereklidir.");

            RuleFor(x => x.AirportId)
                .GreaterThan(0).WithMessage("Geçerli bir havalimanı ID'si gereklidir.");

            RuleFor(x => x.VehicleId)
                .GreaterThan(0).WithMessage("Geçerli bir araç ID'si gereklidir.");

            RuleFor(x => x.PickupCityId)
                .GreaterThan(0).WithMessage("Geçerli bir alış şehri ID'si gereklidir.");

            RuleFor(x => x.DropoffCityId)
                .GreaterThan(0).WithMessage("Geçerli bir bırakış şehri ID'si gereklidir.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Durum gereklidir.")
                .Must(BeValidStatus).WithMessage("Geçerli bir durum seçiniz (Pending, Confirmed, Completed, Cancelled).");

            RuleFor(x => x.Note)
                .MaximumLength(500).WithMessage("Not en fazla 500 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.Note));

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

        private bool BeValidStatus(string status)
        {
            var validStatuses = new[] { "Pending", "Confirmed", "Completed", "Cancelled" };
            return validStatuses.Contains(status);
        }
    }
}

