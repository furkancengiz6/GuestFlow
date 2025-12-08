using FluentValidation;
using GuestFlow.Api.Models.TransferModel;

namespace GuestFlow.Api.Validators
{
    public class AssignVehicleRequestValidator : AbstractValidator<AssignVehicleRequest>
    {
        public AssignVehicleRequestValidator()
        {
            RuleFor(x => x.VehicleId)
                .GreaterThan(0).WithMessage("Geçerli bir araç ID'si gereklidir.");
        }
    }
}

