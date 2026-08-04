using TrackMuvi.Api.TmdbModels;

namespace TrackMuvi.Api.Services;

/// <summary>Cliente tipado hacia api.themoviedb.org. Devuelve los modelos JSON crudos de TMDb;
/// el mapeo a los DTOs que ve el cliente MAUI vive en TrackMuvi.Api/Mapping.</summary>
public interface ITmdbClient
{
    Task<TmdbSearchMultiResponse> SearchMultiAsync(string query, int page, CancellationToken ct);

    /// <summary>
    /// Reemplaza a /movie/upcoming (TMDb): ese endpoint no filtra de forma confiable por fecha
    /// y solo da una ventana corta fija. /discover/movie con rango explícito sí permite pedir
    /// cualquier mes (pasado o futuro) con resultados realmente acotados a esas fechas.
    /// </summary>
    Task<TmdbMovieListResponse> DiscoverMoviesByDateRangeAsync(DateOnly from, DateOnly to, int page, CancellationToken ct);

    /// <summary>
    /// discover/movie ordenado por popularidad, sin tope superior de fecha. Se usa para encontrar
    /// películas confirmadas para un año futuro pero sin fecha exacta todavía (ver
    /// TrackMuvi.Shared.Models.ReleaseDatePrecision): TMDb no tiene un filtro directo para eso, así
    /// que hay que recorrer varias páginas por popularidad y filtrar el patrón en memoria.
    /// </summary>
    Task<TmdbMovieListResponse> DiscoverFutureMoviesByPopularityAsync(DateOnly from, int page, CancellationToken ct);

    /// <summary>discover/movie ordenado por nota de crítica (vote_average), para "Top en críticas".
    /// Se exige un mínimo de votos (ver TmdbClient) para que no gane una película con 3 votos y 10/10.</summary>
    Task<TmdbMovieListResponse> DiscoverTopRatedMoviesAsync(int page, CancellationToken ct);

    Task<TmdbMovieListResponse> GetTrendingMoviesAsync(CancellationToken ct);
    Task<TmdbTvListResponse> GetTrendingTvAsync(CancellationToken ct);

    /// <summary>"Porque viste/marcaste X" — recomendaciones de TMDb basadas en un título puntual,
    /// usadas para armar "Descubre algo nuevo" a partir de lo que el usuario ya marcó como
    /// favorita/vista.</summary>
    Task<TmdbMovieListResponse> GetMovieRecommendationsAsync(int tmdbId, CancellationToken ct);
    Task<TmdbTvListResponse> GetTvRecommendationsAsync(int tmdbId, CancellationToken ct);
    Task<TmdbMovieDetail?> GetMovieDetailAsync(int tmdbId, CancellationToken ct);

    /// <summary>
    /// movie/{id} sin append_to_response: mismos campos base que GetMovieDetailAsync (release_date,
    /// status, overview, runtime, poster, genres, production_companies/belongs_to_collection para
    /// Universe) pero sin pagar el costo de credits/videos/watch-providers/images/release_dates
    /// cuando el llamador no los necesita (listas/resúmenes, chequeo de notificaciones, etc.).
    /// </summary>
    Task<TmdbMovieDetail?> GetMovieBasicAsync(int tmdbId, CancellationToken ct);

    /// <summary>
    /// Endpoint liviano de solo watch/providers (sin credits/videos/images como el detalle completo).
    /// Se usa para saber la plataforma real de cada película en listas (calendario, próximos estrenos)
    /// sin pagar el costo de un GetMovieDetailAsync completo por cada una.
    /// </summary>
    Task<TmdbWatchProvidersResponse?> GetMovieWatchProvidersAsync(int tmdbId, CancellationToken ct);
    Task<TmdbTvDetail?> GetTvDetailAsync(int tmdbId, CancellationToken ct);

    /// <summary>tv/{id} sin append_to_response, ver GetMovieBasicAsync.</summary>
    Task<TmdbTvDetail?> GetTvBasicAsync(int tmdbId, CancellationToken ct);
    Task<TmdbSeasonDetail?> GetSeasonDetailAsync(int tvId, int seasonNumber, CancellationToken ct);
    Task<List<TmdbGenre>> GetMovieGenresAsync(CancellationToken ct);
    Task<List<TmdbGenre>> GetTvGenresAsync(CancellationToken ct);
}
