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

    public Task<TmdbMovieListResponse> GetUpcomingMoviesAsync(int page, CancellationToken ct) =>
        GetAsync<TmdbMovieListResponse>(
            $"movie/upcoming?page={page}&language={_options.DefaultLanguage}&region={_options.DefaultRegion}",
            ct);

    public Task<TmdbMovieListResponse> GetTrendingMoviesAsync(CancellationToken ct) =>
        GetAsync<TmdbMovieListResponse>($"trending/movie/week?language={_options.DefaultLanguage}", ct);

    public Task<TmdbTvListResponse> GetTrendingTvAsync(CancellationToken ct) =>
        GetAsync<TmdbTvListResponse>($"trending/tv/week?language={_options.DefaultLanguage}", ct);

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
