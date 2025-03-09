using DAL.Abstractions.Interfaces;
using DAL.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DAL;

public static class ServiceCollectionExtensions
{
    public static void AddDataAccessLayer(this IServiceCollection repositories)
    {
        repositories.AddScoped<IMusicRepository, MusicRepository>();
        repositories.AddScoped<IUserRepository, UserRepository>();
        repositories.AddScoped<IRoomRepository, RoomRepository>();
    }
}