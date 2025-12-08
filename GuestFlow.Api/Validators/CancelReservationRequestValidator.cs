using FluentValidation;
using GuestFlow.Api.Models.ReservationModels;

namespace GuestFlow.Api.Validators
{
    public class CancelReservationRequestValidator : AbstractValidator<CancelReservationRequest>
    {
        public CancelReservationRequestValidator()
        {
            RuleFor(x => x.CancellationReason)
                .MaximumLength(500).WithMessage("İptal nedeni en fazla 500 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.CancellationReason));
        }
    }
}

