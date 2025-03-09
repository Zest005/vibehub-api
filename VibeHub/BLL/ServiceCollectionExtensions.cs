using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
using BLL.Services;
using BLL.Utilities;
using BLL.Validators;
using Core.Models;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BLL;

public static class ServiceCollectionExtensions
{
    public static void AddBusinessLogicLayer(this IServiceCollection services)
    {
        services.AddTransient<IMusicService, MusicService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IRoomService, RoomService>();

        services.AddScoped<IFilterUtility, FilterUtility>();
        
        services.AddScoped<IValidator<Music>, MusicValidator>();
        services.AddScoped<IValidator<User>, UserValidator>();
        services.AddScoped<IValidator<Room>, RoomValidator>();
    }
}