using Microsoft.EntityFrameworkCore;
using TrackMuvi.Data.Entities;

namespace TrackMuvi.Data;

public class TrackMuviDbContext(DbContextOptions<TrackMuviDbContext> options) : DbContext(options)
{
    public DbSet<PersonalStatusEntity> PersonalStatuses => Set<PersonalStatusEntity>();
    public DbSet<ViewHistoryEntity> ViewHistory => Set<ViewHistoryEntity>();
    public DbSet<EpisodeWatchEntity> EpisodeWatches => Set<EpisodeWatchEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersonalStatusEntity>(entity =>
        {
            entity.HasKey(e => e.TitleKey);
        });

        modelBuilder.Entity<ViewHistoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TitleKey);
        });

        modelBuilder.Entity<EpisodeWatchEntity>(entity =>
        {
            entity.HasKey(e => new { e.TitleKey, e.SeasonNumber, e.EpisodeNumber });
        });
    }
}
