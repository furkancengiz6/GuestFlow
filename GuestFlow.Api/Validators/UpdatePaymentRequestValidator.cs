using FluentValidation;
using GuestFlow.Api.Models.PaymentModels;
using GuestFlow.Domain.Entities.Enum;

namespace GuestFlow.Api.Validators
{
    public class UpdatePaymentRequestValidator : AbstractValidator<UpdatePaymentRequest>
    {
        public UpdatePaymentRequestValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Ödeme tutarı 0'dan büyük olmalıdır.")
                .When(x => x.Amount.HasValue);

            RuleFor(x => x.Currency)
                .Length(3).WithMessage("Para birimi 3 karakter olmalıdır.")
                .Must(c => new[] { "TRY", "USD", "EUR", "GBP", "RUB" }.Contains(c.ToUpper()))
                .WithMessage("Geçersiz para birimi. Desteklenen para birimleri: TRY, USD, EUR, GBP, RUB")
                .When(x => !string.IsNullOrEmpty(x.Currency));

            RuleFor(x => x.PaymentMethod)
                .Must(m => PaymentMethodHelper.IsValidMethod(m))
                .WithMessage("Geçersiz ödeme yöntemi. Desteklenen yöntemler: CreditCard, BankTransfer, Cash, Other")
                .When(x => !string.IsNullOrEmpty(x.PaymentMethod));

            RuleFor(x => x.Status)
                .Must(s => PaymentStatusHelper.IsValidStatus(s))
                .WithMessage("Geçersiz ödeme durumu. Desteklenen durumlar: Pending, Completed, Failed, Refunded, Cancelled")
                .When(x => !string.IsNullOrEmpty(x.Status));

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notlar en fazla 1000 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}

