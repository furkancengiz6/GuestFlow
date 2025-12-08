using FluentValidation;
using GuestFlow.Api.Models.VehicleModels;

namespace GuestFlow.Api.Validators
{
    public class UpdateVehicleRequestValidator : AbstractValidator<UpdateVehicleRequest>
    {
        public UpdateVehicleRequestValidator()
        {
            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Araç tipi gereklidir.")
                .MaximumLength(50).WithMessage("Araç tipi en fazla 50 karakter olabilir.");

            RuleFor(x => x.PlateNumber)
                .NotEmpty().WithMessage("Plaka numarası gereklidir.")
                .MaximumLength(20).WithMessage("Plaka numarası en fazla 20 karakter olabilir.")
                .Matches(@"^[0-9]{1,2}[A-Z]{1,3}[0-9]{2,4}$").WithMessage("Geçerli bir plaka numarası formatı giriniz (örn: 34ABC123).")
                .When(x => !string.IsNullOrEmpty(x.PlateNumber));

            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Kapasite 0'dan büyük olmalıdır.")
                .LessThanOrEqualTo(100).WithMessage("Kapasite 100'den fazla olamaz.");

            RuleFor(x => x.DailyPrice)
                .GreaterThan(0).WithMessage("Günlük fiyat 0'dan büyük olmalıdır.")
                .LessThanOrEqualTo(100000).WithMessage("Günlük fiyat çok yüksek.");
        }
    }
}

