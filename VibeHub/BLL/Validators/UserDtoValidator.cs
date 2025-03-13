using Core.DTO;
using FluentValidation;

namespace BLL.Validators;

public class UserDtoValidator : AbstractValidator<UserDto>
{
    private readonly Dictionary<string, string> _allowedImageMimeTypes = new()
    {
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".bmp", "image/bmp" },
        { ".webp", "image/webp" }
    };

    public UserDtoValidator()
    {
        RuleFor(x => x.Nickname).Length(3, 20)
            .WithMessage("Nickname must be between 3 and 20 characters long");

        RuleFor(x => x.Email).EmailAddress()
            .WithMessage("Email is required and must be a valid email address");

        RuleFor(x => x.Password).Length(5)
            .WithMessage("Password length must be greater than 5");

        RuleFor(x => x.Avatar)
            .Must(file =>
            {
                var extension = Path.GetExtension(file.FileName)?.ToLower();
                return extension != null && _allowedImageMimeTypes.ContainsKey(extension) &&
                       file.ContentType == _allowedImageMimeTypes[extension];
            }).WithMessage("Invalid image format. Allowed formats: jpg, jpeg, png, bmp, webp");
    }
}