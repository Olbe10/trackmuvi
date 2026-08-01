using TrackMuvi.Services.Api;
using TrackMuvi.Services.Notifications;
using TrackMuvi.Services.Personal;
using TrackMuvi.Shared.Enums;

namespace TrackMuvi.Services.BackgroundSync;

public class ReleaseCheckService(
    ITrackMuviApiClient apiClient,
    IPersonalListService personalList,
    INotificationService notifications,
    IKeyValueStore store) : IReleaseCheckService
{
    public async Task RunCheckAsync(CancellationToken ct = default)
    {
        var followingKeys = await personalList.GetKeysByFlagAsync(PersonalStatusFlag.Following, ct);
        await CheckNewEpisodesAsync(followingKeys, ct);

        var wantKeys = await personalList.GetKeysByFlagAsync(PersonalStatusFlag.Want, ct);
        var trackedKeys = followingKeys.Concat(wantKeys).Distinct().ToList();
        await CheckReleaseDatesAsync(trackedKeys, ct);
    }

    private async Task CheckNewEpisodesAsync(IReadOnlyList<string> followingKeys, CancellationToken ct)
    {
        if (followingKeys.Count == 0) return;
        if (store.Get(NotificationPreferenceKeys.NewEpisodes) == "0") return;

        var episodes = await apiClient.GetNextEpisodesAsync(followingKeys, ct);
        var today = DateOnly.FromDateTime(DateTime.Today);

        foreach (var episode in episodes)
        {
            if (episode.AirDate < today || episode.AirDate > today.AddDays(2)) continue;

            var notifiedKey = $"notified-episode:{episode.SeriesKey}:{episode.SeasonNumber}:{episode.EpisodeNumber}";
            if (store.Get(notifiedKey) is not null) continue;

            await notifications.ScheduleNewEpisodeAsync(episode);
            store.Set(notifiedKey, "1");
        }
    }

    private async Task CheckReleaseDatesAsync(IReadOnlyList<string> trackedKeys, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var tomorrow = today.AddDays(1);

        foreach (var key in trackedKeys)
        {
            var detail = await apiClient.GetTitleDetailAsync(key, ct);
            if (detail?.ReleaseDate is not { } releaseDate) continue;

            var dateStoreKey = $"release-date:{key}";
            var previousRaw = store.Get(dateStoreKey);
            if (previousRaw is not null
                && DateOnly.TryParse(previousRaw, out var previousDate)
                && previousDate != releaseDate
                && store.Get(NotificationPreferenceKeys.DateChanges) == "1")
            {
                await notifications.ScheduleDateChangedAsync(detail, previousDate, releaseDate);
            }

            store.Set(dateStoreKey, releaseDate.ToString("yyyy-MM-dd"));

            if (releaseDate == tomorrow && store.Get(NotificationPreferenceKeys.MovieReleases) != "0")
            {
                var tomorrowNotifiedKey = $"notified-tomorrow:{key}:{releaseDate:yyyy-MM-dd}";
                if (store.Get(tomorrowNotifiedKey) is null)
                {
                    await notifications.ScheduleReleaseTomorrowAsync(detail);
                    store.Set(tomorrowNotifiedKey, "1");
                }
            }
        }
    }
}
