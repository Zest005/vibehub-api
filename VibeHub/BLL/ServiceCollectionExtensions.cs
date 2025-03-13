
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
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddSingleton<IFilterUtility, FilterUtility>();
        services.AddSingleton<IMusicFileHelper, MusicFileHelper>();
      
        services.AddScoped<IValidator<User>, UserValidator>();
        services.AddScoped<IValidator<Room>, RoomValidator>();
        services.AddScoped<IValidator<MessageDto>, MessageValidator>();
    }
}