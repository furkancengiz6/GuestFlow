using FluentValidation;
using GuestFlow.Api.Models.CityToursModels;
using System;

namespace GuestFlow.Api.Validators
{
    public class UpdateCityTourRequestValidator : AbstractValidator<UpdateCityTourRequest>
    {
        public UpdateCityTourRequestValidator()
        {
            RuleFor(x => x.TourDate)
                .NotEmpty().WithMessage("Tur tarihi gereklidir.")
                .Must(BeFutureOrToday).WithMessage("Tur tarihi bugünden önceki bir tarih olamaz.");

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
                .GreaterThan(0).WithMessage("Geçerli bir personel ID'si gereklidir.");

            RuleFor(x => x.CityId)
                .GreaterThan(0).WithMessage("Geçerli bir şehir ID'si gereklidir.");
        }

        private bool BeFutureOrToday(DateTime date)
        {
            return date.Date >= DateTime.UtcNow.Date;
        }
    }
}

