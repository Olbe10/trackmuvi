using Microsoft.EntityFrameworkCore;
using TrackMuvi.Data.Entities;

namespace TrackMuvi.Data.Repositories;

public class EpisodeWatchRepository(TrackMuviDbContext db) : IEpisodeWatchRepository
{
    public async Task<HashSet<int>> GetWatchedEpisodeNumbersAsync(string titleKey, int seasonNumber, CancellationToken ct = default) =>
        (await db.EpisodeWatches
            .Where(e => e.TitleKey == titleKey && e.SeasonNumber == seasonNumber)
            .Select(e => e.EpisodeNumber)
            .ToListAsync(ct))
        .ToHashSet();

    public async Task<bool> ToggleEpisodeWatchedAsync(string titleKey, int seasonNumber, int episodeNumber, CancellationToken ct = default)
    {
        var existing = await db.EpisodeWatches.FindAsync([titleKey, seasonNumber, episodeNumber], ct);
        if (existing is not null)
        {
            db.EpisodeWatches.Remove(existing);
            await db.SaveChangesAsync(ct);
            return false;
        }

        db.EpisodeWatches.Add(new EpisodeWatchEntity
        {
            TitleKey = titleKey,
            SeasonNumber = seasonNumber,
            EpisodeNumber = episodeNumber,
            WatchedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task SetEpisodesWatchedAsync(
        string titleKey, int seasonNumber, IEnumerable<int> episodeNumbers, bool watched, CancellationToken ct = default)
    {
        var episodeNumberSet = episodeNumbers.ToHashSet();
        if (episodeNumberSet.Count == 0) return;

        var existing = await db.EpisodeWatches
            .Where(e => e.TitleKey == titleKey && e.SeasonNumber == seasonNumber && episodeNumberSet.Contains(e.EpisodeNumber))
            .ToListAsync(ct);

        if (watched)
        {
            var alreadyWatched = existing.Select(e => e.EpisodeNumber).ToHashSet();
            var now = DateTimeOffset.UtcNow;
            foreach (var episodeNumber in episodeNumberSet.Where(n => !alreadyWatched.Contains(n)))
            {
                db.EpisodeWatches.Add(new EpisodeWatchEntity
                {
                    TitleKey = titleKey,
                    SeasonNumber = seasonNumber,
                    EpisodeNumber = episodeNumber,
                    WatchedAt = now
                });
            }
        }
        else
        {
            db.EpisodeWatches.RemoveRange(existing);
        }

        await db.SaveChangesAsync(ct);
    }
}
