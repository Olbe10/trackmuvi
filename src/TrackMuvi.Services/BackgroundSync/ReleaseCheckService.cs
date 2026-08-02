using TrackMuvi.Data.Entities;
using TrackMuvi.Data.Repositories;
using TrackMuvi.Services.Api;
using TrackMuvi.Services.Notifications;
using TrackMuvi.Services.Personal;
using TrackMuvi.Shared.Enums;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Services.BackgroundSync;

/// <summary>
/// Sincroniza estrenos/episodios de lo que el usuario sigue y agenda las notificaciones
/// correspondientes. Cada título sincronizado queda en TitleCache (SQLite local): eso es lo que
/// permite comparar "¿cambió la fecha?" sin depender de que la API esté disponible en ese momento
/// (si falla la llamada, simplemente no se actualiza nada este ciclo y se reintenta en el próximo;
/// lo que ya se agendó antes con INotificationService sigue en pie porque vive en el SO, no acá).
/// </summary>
public class ReleaseCheckService(
    ITrackMuviApiClient apiClient,
    IPersonalListService personalList,
    INotificationService notifications,
    ITitleCacheRepository titleCache,
    IKeyValueStore store) : IReleaseCheckService
{
    public async Task RunCheckAsync(CancellationToken ct = default)
    {
        var followingKeys = await personalList.GetKeysByFlagAsync(PersonalStatusFlag.Following, ct);
        var wantKeys = await personalList.GetKeysByFlagAsync(PersonalStatusFlag.Want, ct);
        var movieKeys = followingKeys.Concat(wantKeys)
            .Distinct()
            .Where(k => TitleKey.Parse(k).Type == TitleType.Movie)
            .ToList();

        await SyncEpisodesAsync(followingKeys, ct);
        await SyncMoviesAsync(movieKeys, ct);
        await RemoveStaleEntriesAsync(followingKeys.Concat(movieKeys).ToHashSet(), ct);
    }

    public async Task<IReadOnlyList<TodayReleaseDto>> GetTodayReleasesAsync(CancellationToken ct = default)
    {
        await RunCheckAsync(ct);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var cached = await titleCache.GetAllAsync(ct);

        return cached
            .Where(e => e.ReleaseDate == today || e.NextEpisodeAirDate == today)
            .Select(e => new TodayReleaseDto(
                e.TitleKey, e.Type, e.Title, e.PosterPath,
                e.NextEpisodeSeason, e.NextEpisodeNumber, e.NextEpisodeName))
            .ToList();
    }

    private async Task SyncEpisodesAsync(IReadOnlyList<string> followingKeys, CancellationToken ct)
    {
        if (followingKeys.Count == 0) return;

        IReadOnlyList<UpcomingEpisodeDto> episodes;
        try
        {
            episodes = await apiClient.GetNextEpisodesAsync(followingKeys, ct);
        }
        catch
        {
            return; // sin red/API: lo ya agendado en syncs anteriores sigue en pie
        }

        var notifyNewEpisodes = store.Get(NotificationPreferenceKeys.NewEpisodes) != "0";

        foreach (var episode in episodes)
        {
            var cached = await titleCache.GetAsync(episode.SeriesKey, ct);
            var isNewEpisode = cached?.NextEpisodeSeason != episode.SeasonNumber
                || cached?.NextEpisodeNumber != episode.EpisodeNumber
                || cached?.NextEpisodeAirDate != episode.AirDate;

            await titleCache.UpsertAsync(new TitleCacheEntity
            {
                TitleKey = episode.SeriesKey,
                Type = TitleType.Series,
                Title = episode.SeriesTitle,
                PosterPath = episode.SeriesPosterPath,
                NextEpisodeSeason = episode.SeasonNumber,
                NextEpisodeNumber = episode.EpisodeNumber,
                NextEpisodeName = episode.EpisodeName,
                NextEpisodeAirDate = episode.AirDate,
                LastSyncedAt = DateTimeOffset.UtcNow,
            }, ct);

            if (isNewEpisode && notifyNewEpisodes)
            {
                await notifications.ScheduleEpisodeAsync(
                    episode.SeriesKey, episode.SeriesTitle, episode.SeasonNumber, episode.EpisodeNumber,
                    episode.EpisodeName, episode.AirDate);
            }
        }
    }

    private async Task SyncMoviesAsync(IReadOnlyList<string> movieKeys, CancellationToken ct)
    {
        if (movieKeys.Count == 0) return;

        var notifyReleases = store.Get(NotificationPreferenceKeys.MovieReleases) != "0";
        var notifyDateChanges = store.Get(NotificationPreferenceKeys.DateChanges) == "1";

        foreach (var key in movieKeys)
        {
            TitleDetailDto? detail;
            try
            {
                detail = await apiClient.GetTitleDetailAsync(key, ct);
            }
            catch
            {
                continue; // sin red/API: se reintenta en el próximo ciclo, no se pierde lo agendado
            }

            if (detail?.ReleaseDate is not { } releaseDate) continue;

            var cached = await titleCache.GetAsync(key, ct);
            var previousDate = cached?.ReleaseDate;

            await titleCache.UpsertAsync(new TitleCacheEntity
            {
                TitleKey = key,
                Type = TitleType.Movie,
                Title = detail.Title,
                PosterPath = detail.PosterPath,
                ReleaseDate = releaseDate,
                LastSyncedAt = DateTimeOffset.UtcNow,
            }, ct);

            if (previousDate == releaseDate) continue; // sin cambios, nada que avisar/reagendar

            // TMDb marca "solo se sabe el año" con un 1 de enero placeholder (ver
            // ReleaseDatePrecision): mientras siga así no hay una fecha real que agendar.
            if (ReleaseDatePrecision.IsYearOnly(releaseDate)) continue;

            var wasPlaceholder = previousDate is { } prev && ReleaseDatePrecision.IsYearOnly(prev);

            if (previousDate is null || wasPlaceholder)
            {
                // primera fecha real conocida (recién la siguen, o pasó de "solo el año" a fecha real)
                if (wasPlaceholder && notifyReleases)
                    await notifications.NotifyReleaseDateAnnouncedAsync(key, detail.Title, releaseDate);
                if (notifyReleases)
                    await notifications.ScheduleMovieReleaseAsync(key, detail.Title, releaseDate);
            }
            else
            {
                // ya tenía una fecha real y cambió a otra fecha real
                if (notifyDateChanges) await notifications.NotifyDateChangedAsync(key, detail.Title, previousDate.Value, releaseDate);
                if (notifyReleases) await notifications.ScheduleMovieReleaseAsync(key, detail.Title, releaseDate);
            }
        }
    }

    /// <summary>Si el usuario dejó de seguir/querer ver algo, se cancela lo agendado para que no
    /// llegue una notificación de algo que ya no le interesa, y se limpia el cache local.</summary>
    private async Task RemoveStaleEntriesAsync(HashSet<string> trackedKeys, CancellationToken ct)
    {
        var cached = await titleCache.GetAllAsync(ct);
        foreach (var entry in cached)
        {
            if (trackedKeys.Contains(entry.TitleKey)) continue;

            await notifications.CancelAsync(entry.TitleKey);
            await titleCache.RemoveAsync(entry.TitleKey, ct);
        }
    }
}
