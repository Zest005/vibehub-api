using BLL.Abstractions.Interfaces.Services;
using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
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
        services.AddTransient<IMusicService, MusicService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IRoomService, RoomService>();
        services.AddTransient<IMessageHistoryService, MessageHistoryService>();
        
        services.AddScoped<IFilterUtility, FilterUtility>();
        
        services.AddScoped<IValidator<User>, UserValidator>();
        services.AddScoped<IValidator<Room>, RoomValidator>();
        services.AddScoped<IValidator<MessageDto>, MessageValidator>();
        services.AddScoped<IValidator<MusicDto>, MusicDtoValidator>();
    }
}