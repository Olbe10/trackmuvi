using TrackMuvi.Shared.Enums;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Services.Api;

/// <summary>Cliente HTTP hacia TrackMuvi.Api (no habla con TMDb directo: la key vive solo en el backend).</summary>
public interface ITrackMuviApiClient
{
    Task<SearchResultDto> SearchAsync(string query, int page = 1, CancellationToken ct = default);
    /// <summary>Estrenos de cine en un rango de fechas. Sin from/to: hoy + 60 días.</summary>
    Task<IReadOnlyList<TitleSummaryDto>> GetUpcomingMoviesAsync(
        DateOnly? from = null, DateOnly? to = null, int page = 1, CancellationToken ct = default);
    Task<IReadOnlyList<TitleSummaryDto>> GetTrendingMoviesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TitleSummaryDto>> GetTrendingTvAsync(CancellationToken ct = default);
    Task<TitleDetailDto?> GetTitleDetailAsync(string titleKey, CancellationToken ct = default);
    Task<IReadOnlyList<UpcomingEpisodeDto>> GetNextEpisodesAsync(IReadOnlyList<string> seriesKeys, CancellationToken ct = default);
    Task<SeasonDto?> GetSeasonAsync(string seriesKey, int seasonNumber, CancellationToken ct = default);
    Task<IReadOnlyList<GenreDto>> GetGenresAsync(TitleType type, CancellationToken ct = default);
}
