using Core.Models;
using DAL.Abstractions.Interfaces;
using DAL.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace DAL;

public static class ServiceCollectionExtensions
{
    public static void AddDataAccessLayer(this IServiceCollection repositories)
    {
        repositories.AddScoped<IMusicRepository, MusicRepository>();
    }
}