using Microsoft.EntityFrameworkCore;

namespace MicroNetflix.Shared;

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
