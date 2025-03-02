using Core.Models;
using FluentValidation;

namespace BLL.Validators;

public class MusicValidator : AbstractValidator<Music>
{
    private static readonly string[] AllowedExtensions = [".mp3", ".wav", ".flac", ".aac", ".ogg", ".m4a"];

    public MusicValidator()
    {
        RuleFor(m => m.Title).NotEmpty().WithMessage("Title is required");
        RuleFor(m => m.Artist).NotEmpty().WithMessage("Artist is required");
        RuleFor(m => m.Filename)
            .NotEmpty()
            .WithMessage("Filename is required")
            .Must(filename => filename.Contains('.'))
            .WithMessage("Filename must contain a '.' character")
            .Must(filename => AllowedExtensions.Contains(Path.GetExtension(filename).ToLower()))
            .WithMessage($"Filename must have a valid music file extension: {string.Join(", ", AllowedExtensions)}");
    }
}