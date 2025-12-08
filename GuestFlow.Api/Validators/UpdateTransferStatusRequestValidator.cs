using FluentValidation;
using GuestFlow.Api.Models.TransferModel;
using System.Linq;

namespace GuestFlow.Api.Validators
{
    public class UpdateTransferStatusRequestValidator : AbstractValidator<UpdateTransferStatusRequest>
    {
        public UpdateTransferStatusRequestValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Durum gereklidir.")
                .Must(BeValidStatus).WithMessage("Geçerli bir durum seçiniz (Pending, Confirmed, InProgress, Completed, Cancelled).");
        }

        private bool BeValidStatus(string status)
        {
            var validStatuses = new[] { "Pending", "Confirmed", "InProgress", "Completed", "Cancelled" };
            return validStatuses.Contains(status);
        }
    }
}

