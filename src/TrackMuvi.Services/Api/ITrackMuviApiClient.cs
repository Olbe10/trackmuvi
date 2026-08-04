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
    /// <summary>"Top en críticas": catálogo paginado ordenado por nota de TMDb.</summary>
    Task<IReadOnlyList<TitleSummaryDto>> GetTopRatedMoviesAsync(int page = 1, CancellationToken ct = default);
    /// <summary>"Muy pronto": películas confirmadas para un año futuro pero sin fecha de estreno
    /// específica todavía.</summary>
    Task<IReadOnlyList<TitleSummaryDto>> GetComingSoonMoviesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TitleSummaryDto>> GetTrendingTvAsync(CancellationToken ct = default);
    /// <summary>full=false trae solo los campos base de TMDb (sin credits/videos/watch-providers/
    /// images), para listas/resúmenes que no necesitan la ficha completa.</summary>
    Task<TitleDetailDto?> GetTitleDetailAsync(string titleKey, bool full = true, CancellationToken ct = default);
    Task<IReadOnlyList<UpcomingEpisodeDto>> GetNextEpisodesAsync(IReadOnlyList<string> seriesKeys, CancellationToken ct = default);
    /// <summary>"Descubre algo nuevo": recomendaciones de TMDb a partir de un puñado de títulos base
    /// (favoritas/vistas del usuario).</summary>
    Task<IReadOnlyList<TitleSummaryDto>> GetRecommendationsAsync(IReadOnlyList<string> basisTitleKeys, CancellationToken ct = default);
    Task<SeasonDto?> GetSeasonAsync(string seriesKey, int seasonNumber, CancellationToken ct = default);
    Task<IReadOnlyList<GenreDto>> GetGenresAsync(TitleType type, CancellationToken ct = default);
}
