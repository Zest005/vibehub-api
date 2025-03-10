using Core.DTO;
using FluentValidation;

namespace BLL.Validators;

public class MusicDtoValidator : AbstractValidator<MusicDto>
{
    public MusicDtoValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .Must(file => file != null && (file.FileName.EndsWith(".flac") || file.FileName.EndsWith(".wav") ||
                                           file.FileName.EndsWith(".mp3")))
            .WithMessage("File extension should be flac, wav or mp4");
    }
}