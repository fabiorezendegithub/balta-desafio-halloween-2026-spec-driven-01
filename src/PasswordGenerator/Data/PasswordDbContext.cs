using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Domain;

namespace PasswordGenerator.Data;

public class PasswordDbContext(DbContextOptions<PasswordDbContext> options) : DbContext(options)
{
    public DbSet<PasswordEntry> Passwords => Set<PasswordEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PasswordEntry>(entity =>
        {
            entity.ToTable("Passwords");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Guid).IsRequired();
            entity.Property(e => e.Password).IsRequired();
            entity.HasIndex(e => e.Guid).IsUnique();
            entity.HasIndex(e => e.Password).IsUnique();
        });
    }
}
