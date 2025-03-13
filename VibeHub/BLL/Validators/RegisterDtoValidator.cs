using Core.DTO;
using FluentValidation;

namespace BLL.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Nickname)
            .NotEmpty()
            .Length(3, 20)
            .Matches(@"^[A-Za-z0-9]+$") 
            .WithMessage("Nickname must be between 3 and 20 characters long and contain only letters and numbers.");

        RuleFor(x => x.Email).NotEmpty().EmailAddress()
            .WithMessage("Email is required and must be a valid email address");
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(5)
            .Matches(@"^(?=.*[A-Za-z])(?=.*\d).+$")
            .WithMessage("Password must contain at least one letter and one number.");
    }
}