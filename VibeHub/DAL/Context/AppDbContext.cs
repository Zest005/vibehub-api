using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        Database.OpenConnection();
    }

    public DbSet<Music> Musics { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<MessageHistory> MessageHistories { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RoomSettings> RoomSettings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost:5432;Database=VibeHub;Username=postgres;Password=root");
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}