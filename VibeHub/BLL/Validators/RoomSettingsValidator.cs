using Core.Models;
using FluentValidation;

namespace BLL.Validators;

public class RoomSettingsValidator : AbstractValidator<RoomSettings>
{
    public RoomSettingsValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required when the room is private.")
            .When(x => !x.Availability);
    }
}