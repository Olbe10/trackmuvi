using TrackMuvi.Api.TmdbModels;

namespace TrackMuvi.Api.Services;

/// <summary>Cliente tipado hacia api.themoviedb.org. Devuelve los modelos JSON crudos de TMDb;
/// el mapeo a los DTOs que ve el cliente MAUI vive en TrackMuvi.Api/Mapping.</summary>
public interface ITmdbClient
{
    Task<TmdbSearchMultiResponse> SearchMultiAsync(string query, int page, CancellationToken ct);
    Task<TmdbMovieListResponse> GetUpcomingMoviesAsync(int page, CancellationToken ct);
    Task<TmdbMovieListResponse> GetTrendingMoviesAsync(CancellationToken ct);
    Task<TmdbTvListResponse> GetTrendingTvAsync(CancellationToken ct);
    Task<TmdbMovieDetail?> GetMovieDetailAsync(int tmdbId, CancellationToken ct);
    Task<TmdbTvDetail?> GetTvDetailAsync(int tmdbId, CancellationToken ct);
    Task<TmdbSeasonDetail?> GetSeasonDetailAsync(int tvId, int seasonNumber, CancellationToken ct);
    Task<List<TmdbGenre>> GetMovieGenresAsync(CancellationToken ct);
    Task<List<TmdbGenre>> GetTvGenresAsync(CancellationToken ct);
}
