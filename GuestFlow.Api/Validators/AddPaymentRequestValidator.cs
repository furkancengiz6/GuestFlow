using FluentValidation;
using GuestFlow.Api.Models.PaymentModels;
using GuestFlow.Domain.Entities.Enum;

namespace GuestFlow.Api.Validators
{
    public class AddPaymentRequestValidator : AbstractValidator<AddPaymentRequest>
    {
        public AddPaymentRequestValidator()
        {
            RuleFor(x => x.InvoiceId)
                .GreaterThan(0).WithMessage("Fatura ID 0'dan büyük olmalıdır.");

            RuleFor(x => x.GuestId)
                .GreaterThan(0).WithMessage("Misafir ID 0'dan büyük olmalıdır.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Ödeme tutarı 0'dan büyük olmalıdır.");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Para birimi gereklidir.")
                .Length(3).WithMessage("Para birimi 3 karakter olmalıdır.")
                .Must(c => new[] { "TRY", "USD", "EUR", "GBP", "RUB" }.Contains(c.ToUpper()))
                .WithMessage("Geçersiz para birimi. Desteklenen para birimleri: TRY, USD, EUR, GBP, RUB");

            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("Ödeme yöntemi gereklidir.")
                .Must(m => PaymentMethodHelper.IsValidMethod(m))
                .WithMessage("Geçersiz ödeme yöntemi. Desteklenen yöntemler: CreditCard, BankTransfer, Cash, Other");

            RuleFor(x => x.PaymentDate)
                .NotEmpty().WithMessage("Ödeme tarihi gereklidir.");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notlar en fazla 1000 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}

