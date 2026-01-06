using FluentValidation;
using GuestFlow.Api.Models.TransferModel;
using System;
using System.Linq;

namespace GuestFlow.Api.Validators
{
    public class UpdateTransferRequestValidator : AbstractValidator<UpdateTransferRequest>
    {
        public UpdateTransferRequestValidator()
        {
            // DATE REALITY: Past-dated entries are allowed for retroactive data entry
            // Service date represents when the operation ACTUALLY occurred
            RuleFor(x => x.TransferDate)
                .NotEmpty().WithMessage("Transfer tarihi gereklidir.");

            RuleFor(x => x.PickupAddress)
                .NotEmpty().WithMessage("Alış adresi gereklidir.")
                .MaximumLength(500).WithMessage("Alış adresi en fazla 500 karakter olabilir.");

            RuleFor(x => x.DropoffAddress)
                .NotEmpty().WithMessage("Bırakış adresi gereklidir.")
                .MaximumLength(500).WithMessage("Bırakış adresi en fazla 500 karakter olabilir.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.")
                .LessThanOrEqualTo(1000000).WithMessage("Fiyat çok yüksek.");

            RuleFor(x => x.GuestId)
                .GreaterThan(0).WithMessage("Geçerli bir misafir ID'si gereklidir.");

            RuleFor(x => x.PersonnelId)
                .GreaterThan(0).WithMessage("Geçerli bir personel ID'si gereklidir.")
                .When(x => x.PersonnelId.HasValue);

            RuleFor(x => x.AirportId)
                .GreaterThan(0).WithMessage("Geçerli bir havalimanı ID'si gereklidir.")
                .When(x => x.AirportId.HasValue);

            RuleFor(x => x.VehicleId)
                .GreaterThan(0).WithMessage("Geçerli bir araç ID'si gereklidir.")
                .When(x => x.VehicleId.HasValue);

            RuleFor(x => x.PickupCityId)
                .GreaterThan(0).WithMessage("Geçerli bir alış şehri ID'si gereklidir.")
                .When(x => x.PickupCityId.HasValue);

            RuleFor(x => x.DropoffCityId)
                .GreaterThan(0).WithMessage("Geçerli bir bırakış şehri ID'si gereklidir.")
                .When(x => x.DropoffCityId.HasValue);

            RuleFor(x => x.HotelId)
                .GreaterThan(0).WithMessage("Geçerli bir otel ID'si gereklidir.")
                .When(x => x.HotelId.HasValue);

            RuleFor(x => x.RestaurantId)
                .GreaterThan(0).WithMessage("Geçerli bir restoran ID'si gereklidir.")
                .When(x => x.RestaurantId.HasValue);

            RuleFor(x => x.Status)
                .Must(BeValidStatus).WithMessage("Geçerli bir durum seçiniz (Pending, Confirmed, InProgress, Completed, Cancelled).")
                .When(x => !string.IsNullOrEmpty(x.Status));

            RuleFor(x => x.DiscountPercentage)
                .InclusiveBetween(0, 100).WithMessage("İndirim yüzdesi 0 ile 100 arasında olmalıdır.")
                .When(x => x.DiscountPercentage.HasValue);

            RuleFor(x => x.Currency)
                .Must(BeValidCurrencyCode).WithMessage("Geçerli bir para birimi kodu giriniz (TRY, USD, EUR, GBP, RUB).")
                .When(x => !string.IsNullOrEmpty(x.Currency));

            RuleFor(x => x.Note)
                .MaximumLength(500).WithMessage("Not en fazla 500 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.Note));

            RuleFor(x => x.SupplierName)
                .MaximumLength(200).WithMessage("Tedarikçi adı en fazla 200 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.SupplierName));

            RuleFor(x => x.SupplierCurrency)
                .MaximumLength(3).WithMessage("Tedarikçi para birimi 3 karakter olmalıdır.")
                .When(x => !string.IsNullOrEmpty(x.SupplierCurrency));

            RuleFor(x => x.SupplierPaymentStatus)
                .MaximumLength(20).WithMessage("Tedarikçi ödeme durumu en fazla 20 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.SupplierPaymentStatus));

            RuleFor(x => x.SupplierInvoiceNumber)
                .MaximumLength(100).WithMessage("Tedarikçi fatura numarası en fazla 100 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.SupplierInvoiceNumber));
        }

        // BeFutureOrToday REMOVED - Past-dated entries are allowed per DATE REALITY
        // Service date represents when the operation actually occurred, not when entered

        private bool BeValidStatus(string status)
        {
            var validStatuses = new[] { "Pending", "Confirmed", "InProgress", "Completed", "Cancelled" };
            return validStatuses.Contains(status);
        }

        private bool BeValidCurrencyCode(string? currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
                return true; // Opsiyonel alan, boş olabilir

            var validCodes = new[] { "TRY", "USD", "EUR", "GBP", "RUB" };
            return validCodes.Contains(currencyCode.ToUpper());
        }
    }
}

