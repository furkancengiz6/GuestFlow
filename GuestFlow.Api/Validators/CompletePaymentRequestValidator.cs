using FluentValidation;
using GuestFlow.Api.Models.PaymentModels;

namespace GuestFlow.Api.Validators
{
    public class CompletePaymentRequestValidator : AbstractValidator<CompletePaymentRequest>
    {
        public CompletePaymentRequestValidator()
        {
            RuleFor(x => x.TransactionId)
                .NotEmpty().WithMessage("Transaction ID gereklidir.")
                .MaximumLength(200).WithMessage("Transaction ID en fazla 200 karakter olabilir.");
        }
    }
}

