using Microsoft.EntityFrameworkCore;
using TrackMuvi.Data.Entities;

namespace TrackMuvi.Data.Repositories;

public class TitleCacheRepository(TrackMuviDbContext db) : ITitleCacheRepository
{
    public async Task<TitleCacheEntity?> GetAsync(string titleKey, CancellationToken ct = default) =>
        await db.TitleCache.FindAsync([titleKey], ct);

    public async Task<IReadOnlyList<TitleCacheEntity>> GetAllAsync(CancellationToken ct = default) =>
        await db.TitleCache.ToListAsync(ct);

    public async Task UpsertAsync(TitleCacheEntity entity, CancellationToken ct = default)
    {
        var existing = await db.TitleCache.FindAsync([entity.TitleKey], ct);
        if (existing is null)
        {
            db.TitleCache.Add(entity);
        }
        else
        {
            db.Entry(existing).CurrentValues.SetValues(entity);
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(string titleKey, CancellationToken ct = default)
    {
        var existing = await db.TitleCache.FindAsync([titleKey], ct);
        if (existing is null) return;

        db.TitleCache.Remove(existing);
        await db.SaveChangesAsync(ct);
    }
}
