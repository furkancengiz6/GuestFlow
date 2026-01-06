using FluentValidation;
using GuestFlow.Api.Models.CityTourModels;
using System;

namespace GuestFlow.Api.Validators
{
    public class UpdateCityTourRequestValidator : AbstractValidator<UpdateCityTourRequest>
    {
        public UpdateCityTourRequestValidator()
        {
            // DATE REALITY: Past-dated entries are allowed for retroactive data entry
            // Service date represents when the operation ACTUALLY occurred
            RuleFor(x => x.TourDate)
                .NotEmpty().WithMessage("Tur tarihi gereklidir.");

            RuleFor(x => x.Language)
                .NotEmpty().WithMessage("Dil gereklidir.")
                .MaximumLength(50).WithMessage("Dil en fazla 50 karakter olabilir.");

            RuleFor(x => x.DurationHours)
                .GreaterThan(0).WithMessage("Süre 0'dan büyük olmalıdır.")
                .LessThanOrEqualTo(24).WithMessage("Süre 24 saatten fazla olamaz.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.")
                .LessThanOrEqualTo(1000000).WithMessage("Fiyat çok yüksek.");

            RuleFor(x => x.OwnerGuestId)
                .GreaterThan(0).WithMessage("Geçerli bir misafir ID'si gereklidir.");

            RuleFor(x => x.PersonnelId)
                .GreaterThan(0).WithMessage("Geçerli bir personel ID'si gereklidir.")
                .When(x => x.PersonnelId.HasValue);

            RuleFor(x => x.CityId)
                .GreaterThan(0).WithMessage("Geçerli bir şehir ID'si gereklidir.");

            // Opsiyonel alanlar için uzunluk kontrolleri
            RuleFor(x => x.DriverName)
                .MaximumLength(200).WithMessage("Şoför adı en fazla 200 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.DriverName));

            RuleFor(x => x.GuideName)
                .MaximumLength(200).WithMessage("Rehber adı en fazla 200 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.GuideName));

            RuleFor(x => x.GuidePhone)
                .MaximumLength(20).WithMessage("Rehber telefonu en fazla 20 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.GuidePhone));

            RuleFor(x => x.ExternalVehiclePlate)
                .MaximumLength(20).WithMessage("Araç plakası en fazla 20 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.ExternalVehiclePlate));

            RuleFor(x => x.ExternalDriverName)
                .MaximumLength(200).WithMessage("Şoför adı en fazla 200 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.ExternalDriverName));

            RuleFor(x => x.ExternalDriverPhone)
                .MaximumLength(20).WithMessage("Şoför telefonu en fazla 20 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.ExternalDriverPhone));

            RuleFor(x => x.CaptainPhone)
                .MaximumLength(20).WithMessage("Kaptan telefonu en fazla 20 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.CaptainPhone));

            RuleFor(x => x.PaymentNote)
                .MaximumLength(500).WithMessage("Ödeme notu en fazla 500 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.PaymentNote));

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
    }
}

