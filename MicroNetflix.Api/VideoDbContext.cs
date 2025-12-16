using Microsoft.EntityFrameworkCore;
using MicroNetflix.Shared;

namespace MicroNetflix.Api;

public class VideoDbContext : DbContext
{
    public VideoDbContext(DbContextOptions<VideoDbContext> options) : base(options) { }

    public DbSet<VideoMetadata> Videos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VideoMetadata>()
            .HasKey(v => v.Id);
    }
}
