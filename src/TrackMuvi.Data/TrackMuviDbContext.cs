using Microsoft.EntityFrameworkCore;
using TrackMuvi.Data.Entities;

namespace TrackMuvi.Data;

public class TrackMuviDbContext(DbContextOptions<TrackMuviDbContext> options) : DbContext(options)
{
    public DbSet<PersonalStatusEntity> PersonalStatuses => Set<PersonalStatusEntity>();
    public DbSet<ViewHistoryEntity> ViewHistory => Set<ViewHistoryEntity>();

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
    }
}
