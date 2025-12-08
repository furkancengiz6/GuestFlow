using FluentValidation;
using GuestFlow.Api.Models.TransferModel;
using System;
using System.Linq;

namespace GuestFlow.Api.Validators
{
    public class UpdateTransferRequestValidator : AbstractValidator<UpdateTransferRequest>
    {
        public UpdateTransferRequestValidator()
        {
            RuleFor(x => x.TransferDate)
                .NotEmpty().WithMessage("Transfer tarihi gereklidir.")
                .Must(BeFutureOrToday).WithMessage("Transfer tarihi bugünden önceki bir tarih olamaz.");

            RuleFor(x => x.PickupAddress)
                .NotEmpty().WithMessage("Alış adresi gereklidir.")
                .MaximumLength(200).WithMessage("Alış adresi en fazla 200 karakter olabilir.");

            RuleFor(x => x.DropoffAddress)
                .NotEmpty().WithMessage("Bırakış adresi gereklidir.")
                .MaximumLength(200).WithMessage("Bırakış adresi en fazla 200 karakter olabilir.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.")
                .LessThanOrEqualTo(1000000).WithMessage("Fiyat çok yüksek.");

            RuleFor(x => x.GuestId)
                .GreaterThan(0).WithMessage("Geçerli bir misafir ID'si gereklidir.");

            RuleFor(x => x.PersonnelId)
                .GreaterThan(0).WithMessage("Geçerli bir personel ID'si gereklidir.");

            RuleFor(x => x.AirportId)
                .GreaterThan(0).WithMessage("Geçerli bir havalimanı ID'si gereklidir.");

            RuleFor(x => x.VehicleId)
                .GreaterThan(0).WithMessage("Geçerli bir araç ID'si gereklidir.");

            RuleFor(x => x.PickupCityId)
                .GreaterThan(0).WithMessage("Geçerli bir alış şehri ID'si gereklidir.");

            RuleFor(x => x.DropoffCityId)
                .GreaterThan(0).WithMessage("Geçerli bir bırakış şehri ID'si gereklidir.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Durum gereklidir.")
                .Must(BeValidStatus).WithMessage("Geçerli bir durum seçiniz (Pending, Confirmed, InProgress, Completed, Cancelled).");

            RuleFor(x => x.Note)
                .MaximumLength(500).WithMessage("Not en fazla 500 karakter olabilir.")
                .When(x => !string.IsNullOrEmpty(x.Note));
        }

        private bool BeFutureOrToday(DateTime date)
        {
            return date.Date >= DateTime.UtcNow.Date;
        }

        private bool BeValidStatus(string status)
        {
            var validStatuses = new[] { "Pending", "Confirmed", "InProgress", "Completed", "Cancelled" };
            return validStatuses.Contains(status);
        }
    }
}

