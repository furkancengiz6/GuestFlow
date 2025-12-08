using FluentValidation;
using GuestFlow.Api.Models.ReservationModels;

namespace GuestFlow.Api.Validators
{
    public class AddReservationRequestValidator : AbstractValidator<AddReservationRequest>
    {
        public AddReservationRequestValidator()
        {
            RuleFor(x => x.GuestId)
                .GreaterThan(0).WithMessage("Misafir ID 0'dan büyük olmalıdır.");

            RuleFor(x => x.PersonnelId)
                .GreaterThan(0).WithMessage("Personel ID 0'dan büyük olmalıdır.");

            RuleFor(x => x.ServiceType)
                .NotEmpty().WithMessage("Servis tipi gereklidir.")
                .Must(st => st == "Transfer" || st == "CityTour" || st == "YachtTour")
                .WithMessage("Servis tipi Transfer, CityTour veya YachtTour olmalıdır.");

            RuleFor(x => x.ServiceId)
                .GreaterThan(0).WithMessage("Servis ID 0'dan büyük olmalıdır.");

            RuleFor(x => x.ReservationDate)
                .NotEmpty().WithMessage("Rezervasyon tarihi gereklidir.")
                .Must(date => date >= System.DateTime.Today)
                .WithMessage("Rezervasyon tarihi bugünden önceki bir tarih olamaz.");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notlar en fazla 1000 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}

