using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DAL.Context;

public class AppDbContext : DbContext
{
    public DbSet<Music> Musics { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<MessageHistory> MessageHistories { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RoomSettings> RoomSettings { get; set; }
    public DbSet<RoomsMusics> RoomsMusics { get; set; }
    public DbSet<Guest> Guests { get; set; }

    private readonly IConfiguration _configuration;

    public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<RoomsMusics>()
            .HasKey(rm => new { rm.MusicId, rm.RoomId });
        
        base.OnModelCreating(builder);
    }
}