using System.Net;
using Microsoft.Extensions.Options;
using TrackMuvi.Api.Options;
using TrackMuvi.Api.TmdbModels;

namespace TrackMuvi.Api.Services;

public class TmdbClient(HttpClient httpClient, IOptions<TmdbOptions> options) : ITmdbClient
{
    private readonly TmdbOptions _options = options.Value;

    // Rutas SIN "/" inicial a propósito: HttpClient.BaseAddress termina en "/3/" y una ruta
    // relativa que empiece con "/" descarta ese segmento en vez de combinarse con él.
    public Task<TmdbSearchMultiResponse> SearchMultiAsync(string query, int page, CancellationToken ct) =>
        GetAsync<TmdbSearchMultiResponse>(
            $"search/multi?query={Uri.EscapeDataString(query)}&include_adult=false&page={page}&language={_options.DefaultLanguage}",
            ct);

    public Task<TmdbMovieListResponse> DiscoverMoviesByDateRangeAsync(DateOnly from, DateOnly to, int page, CancellationToken ct) =>
        GetAsync<TmdbMovieListResponse>(
            "discover/movie" +
            // popularity.desc en vez de primary_release_date.asc: TMDb tiene cientos de estrenos
            // obscuros/festivaleros por mes, y con solo 2 páginas (40 resultados) ordenar por fecha
            // los agotaba en los primeros 3-4 días del mes, dejando el resto sin datos.
            $"?sort_by=popularity.desc" +
            $"&with_release_type={Uri.EscapeDataString("2|3")}" +
            $"&primary_release_date.gte={from:yyyy-MM-dd}" +
            $"&primary_release_date.lte={to:yyyy-MM-dd}" +
            $"&page={page}&language={_options.DefaultLanguage}&region={_options.DefaultRegion}",
            ct);

    public Task<TmdbMovieListResponse> GetTrendingMoviesAsync(CancellationToken ct) =>
        GetAsync<TmdbMovieListResponse>($"trending/movie/week?language={_options.DefaultLanguage}", ct);

    public Task<TmdbTvListResponse> GetTrendingTvAsync(CancellationToken ct) =>
        GetAsync<TmdbTvListResponse>($"trending/tv/week?language={_options.DefaultLanguage}", ct);

    public async Task<TmdbWatchProvidersResponse?> GetMovieWatchProvidersAsync(int tmdbId, CancellationToken ct)
    {
        var response = await httpClient.GetAsync($"movie/{tmdbId}/watch/providers", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TmdbWatchProvidersResponse>(cancellationToken: ct);
    }

    public async Task<TmdbMovieDetail?> GetMovieDetailAsync(int tmdbId, CancellationToken ct)
    {
        var response = await httpClient.GetAsync(
            $"movie/{tmdbId}?append_to_response=credits,videos,watch/providers,release_dates,images&language={_options.DefaultLanguage}&include_image_language=null,en",
            ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TmdbMovieDetail>(cancellationToken: ct);
    }

    public async Task<TmdbTvDetail?> GetTvDetailAsync(int tmdbId, CancellationToken ct)
    {
        var response = await httpClient.GetAsync(
            $"tv/{tmdbId}?append_to_response=credits,videos,watch/providers,content_ratings,images&language={_options.DefaultLanguage}&include_image_language=null,en",
            ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TmdbTvDetail>(cancellationToken: ct);
    }

    public async Task<TmdbSeasonDetail?> GetSeasonDetailAsync(int tvId, int seasonNumber, CancellationToken ct)
    {
        var response = await httpClient.GetAsync(
            $"tv/{tvId}/season/{seasonNumber}?language={_options.DefaultLanguage}",
            ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TmdbSeasonDetail>(cancellationToken: ct);
    }

    public async Task<List<TmdbGenre>> GetMovieGenresAsync(CancellationToken ct)
    {
        var result = await GetAsync<TmdbGenreListResponse>($"genre/movie/list?language={_options.DefaultLanguage}", ct);
        return result.Genres;
    }

    public async Task<List<TmdbGenre>> GetTvGenresAsync(CancellationToken ct)
    {
        var result = await GetAsync<TmdbGenreListResponse>($"genre/tv/list?language={_options.DefaultLanguage}", ct);
        return result.Genres;
    }

    private async Task<T> GetAsync<T>(string relativeUrl, CancellationToken ct)
    {
        var response = await httpClient.GetAsync(relativeUrl, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct)
            ?? throw new InvalidOperationException($"TMDb devolvió un cuerpo vacío para '{relativeUrl}'.");
    }
}
