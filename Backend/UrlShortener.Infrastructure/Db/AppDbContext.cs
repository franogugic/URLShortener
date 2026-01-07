using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Db;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Url> Urls { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Konfiguracija za User entitet
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username)
                .IsUnique();
        });

        // Konfiguracija za Url entitet
        modelBuilder.Entity<Url>(entity =>
        {
            entity.HasIndex(u => u.ShortUrlCode)
                .IsUnique();

            // Definiranje relacije 1:N (Jedan korisnik ima više URL-ova)
            entity.HasOne(u => u.User)
                .WithMany(u => u.Urls) // Ovo zahtijeva ICollection<Url> u User klasi
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}