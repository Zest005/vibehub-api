using Core.Models;
using FluentValidation;

namespace BLL.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Nickname).NotEmpty().Length(3, 20)
            .WithMessage("Nickname must be between 3 and 20 characters long");
        RuleFor(x => x.Email).NotEmpty().EmailAddress()
            .WithMessage("Email is required and must be a valid email address");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
    }
}