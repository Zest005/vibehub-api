using Core.Models;
using FluentValidation;

namespace BLL.Validators;

public class RoomValidator : AbstractValidator<Room>
{
    public RoomValidator()
    {
        RuleFor(r => r.OwnerId).NotEmpty().WithMessage("Owner ID is required");
    }
}