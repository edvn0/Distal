using Distal.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Distal.Core.Configuration;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<MeshFile> MeshFiles { get; set; }
    public DbSet<MeshData> MeshData { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<MeshFile>()
               .HasOne(m => m.User)
               .WithMany(u => u.MeshFiles)
               .HasForeignKey(m => m.UserId);

        builder.Entity<User>()
            .HasMany(u => u.MeshFiles)
            .WithOne(m => m.User);

        builder.Entity<User>()
            .HasIndex(e => e.Email)
            .IsUnique();

        builder.Entity<MeshFile>()
           .HasOne(m => m.MeshData)
           .WithOne(d => d.MeshFile)
           .HasForeignKey<MeshData>(d => d.MeshFileId)
           .OnDelete(DeleteBehavior.Cascade);
    }
}