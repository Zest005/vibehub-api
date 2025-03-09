using Core.DTO;
using FluentValidation;

namespace BLL.Validators;

public class MessageValidator : AbstractValidator<MessageDto>
{
    public MessageValidator()
    {
        RuleFor(m => m.Text)
            .Length(1, 100)
            .WithMessage("Message should be not empty. The limit of characters 100.");
    }
}