using FluentValidation;
using GuestFlow.Api.Models.ReservationModels;

namespace GuestFlow.Api.Validators
{
    public class UpdateReservationRequestValidator : AbstractValidator<UpdateReservationRequest>
    {
        public UpdateReservationRequestValidator()
        {
            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notlar en fazla 1000 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.Notes));

            RuleFor(x => x.ReservationDate)
                .Must(date => date.HasValue && date.Value >= System.DateTime.Today)
                .WithMessage("Rezervasyon tarihi bugünden önceki bir tarih olamaz.")
                .When(x => x.ReservationDate.HasValue);
        }
    }
}

