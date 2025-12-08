using FluentValidation;
using GuestFlow.Api.Models.YachtTourModels;
using System;

namespace GuestFlow.Api.Validators
{
    public class UpdateYachtTourRequestValidator : AbstractValidator<UpdateYachtTourRequest>
    {
        public UpdateYachtTourRequestValidator()
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
        }

        private bool BeFutureOrToday(DateTime date)
        {
            return date.Date >= DateTime.UtcNow.Date;
        }
    }
}

