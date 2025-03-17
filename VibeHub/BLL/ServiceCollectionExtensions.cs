using BLL.Abstractions.Helpers;
using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
using BLL.Helpers;
using BLL.Services;
using BLL.Utilities;
using BLL.Validators;
using Core.DTO;
using Core.Models;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BLL;

public static class ServiceCollectionExtensions
{
    public static void AddBusinessLogicLayer(this IServiceCollection services)
    {
        services.AddScoped<IMusicService, MusicService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IMessageHistoryService, MessageHistoryService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IGuestService, GuestService>();


        services.AddSingleton<IFilterUtility, FilterUtility>();
        services.AddSingleton<IMusicFileHelper, MusicFileHelper>();
        services.AddSingleton<IGeneratorUtility, GeneratorUtility>();
        services.AddSingleton<IPasswordManagerUtility, PasswordManagerUtility>();

        services.AddScoped<IValidator<User>, UserValidator>();
        services.AddScoped<IValidator<UserDto>, UserDtoValidator>();
        services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();
        services.AddScoped<IValidator<RoomSettings>, RoomSettingsValidator>();
        services.AddScoped<IValidator<MessageDto>, MessageValidator>();

        services.AddScoped<SessionValidationAttribute>();
    }
}