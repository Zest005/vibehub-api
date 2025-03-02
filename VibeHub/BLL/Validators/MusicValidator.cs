using Core.Models;
using FluentValidation;

namespace BLL.Validators;

public class MusicValidator : AbstractValidator<Music>
{
    public MusicValidator()
    {
        RuleFor(m => m.Title).NotEmpty().WithMessage("Title is required");
        RuleFor(m => m.Artist).NotEmpty().WithMessage("Artist is required");
        RuleFor(m => m.Filename).NotEmpty().WithMessage("Filename is required");
    }
}