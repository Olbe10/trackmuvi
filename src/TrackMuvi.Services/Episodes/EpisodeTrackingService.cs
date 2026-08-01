using TrackMuvi.Data.Repositories;
using TrackMuvi.Services.Api;

namespace TrackMuvi.Services.Episodes;

public class EpisodeTrackingService(ITrackMuviApiClient api, IEpisodeWatchRepository watchRepository) : IEpisodeTrackingService
{
    public async Task<SeasonProgressDto?> GetSeasonAsync(string seriesKey, int seasonNumber, CancellationToken ct = default)
    {
        var season = await api.GetSeasonAsync(seriesKey, seasonNumber, ct);
        if (season is null) return null;

        var watchedNumbers = await watchRepository.GetWatchedEpisodeNumbersAsync(seriesKey, seasonNumber, ct);
        var today = DateOnly.FromDateTime(DateTime.Today);

        var episodes = season.Episodes
            .Select(e => new EpisodeProgressDto(
                e.SeasonNumber, e.EpisodeNumber, e.Name, e.Overview, e.AirDate, e.RuntimeMinutes, e.StillPath,
                Watched: watchedNumbers.Contains(e.EpisodeNumber),
                HasAired: e.AirDate is not null && e.AirDate <= today))
            .OrderBy(e => e.EpisodeNumber)
            .ToList();

        return new SeasonProgressDto(
            seasonNumber, episodes,
            AiredCount: episodes.Count(e => e.HasAired),
            WatchedCount: episodes.Count(e => e.Watched));
    }

    public Task<bool> ToggleEpisodeWatchedAsync(string seriesKey, int seasonNumber, int episodeNumber, CancellationToken ct = default) =>
        watchRepository.ToggleEpisodeWatchedAsync(seriesKey, seasonNumber, episodeNumber, ct);
}
