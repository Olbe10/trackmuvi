using System.Net;
using System.Net.Http.Json;
using TrackMuvi.Shared.Enums;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Services.Api;

public class TrackMuviApiClient(HttpClient httpClient) : ITrackMuviApiClient
{
    public async Task<SearchResultDto> SearchAsync(string query, int page = 1, CancellationToken ct = default) =>
        await httpClient.GetFromJsonAsync<SearchResultDto>(
            $"api/search?query={Uri.EscapeDataString(query)}&page={page}", ct)
        ?? new SearchResultDto([], 1, 0, 0);

    public async Task<IReadOnlyList<TitleSummaryDto>> GetUpcomingMoviesAsync(
        DateOnly? from = null, DateOnly? to = null, int page = 1, CancellationToken ct = default)
    {
        var query = $"api/movies/upcoming?page={page}";
        if (from is { } f) query += $"&from={f:yyyy-MM-dd}";
        if (to is { } t) query += $"&to={t:yyyy-MM-dd}";
        return await httpClient.GetFromJsonAsync<List<TitleSummaryDto>>(query, ct) ?? [];
    }

    public async Task<IReadOnlyList<TitleSummaryDto>> GetTrendingMoviesAsync(CancellationToken ct = default) =>
        await httpClient.GetFromJsonAsync<List<TitleSummaryDto>>("api/movies/trending", ct) ?? [];

    public async Task<IReadOnlyList<TitleSummaryDto>> GetTopRatedMoviesAsync(int page = 1, CancellationToken ct = default) =>
        await httpClient.GetFromJsonAsync<List<TitleSummaryDto>>($"api/movies/top-rated?page={page}", ct) ?? [];

    public async Task<IReadOnlyList<TitleSummaryDto>> GetComingSoonMoviesAsync(CancellationToken ct = default) =>
        await httpClient.GetFromJsonAsync<List<TitleSummaryDto>>("api/movies/coming-soon", ct) ?? [];

    public async Task<IReadOnlyList<TitleSummaryDto>> GetTrendingTvAsync(CancellationToken ct = default) =>
        await httpClient.GetFromJsonAsync<List<TitleSummaryDto>>("api/tv/trending", ct) ?? [];

    public async Task<TitleDetailDto?> GetTitleDetailAsync(string titleKey, bool full = true, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync($"api/titles/by-key/{titleKey}?full={full}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TitleDetailDto>(cancellationToken: ct);
    }

    public async Task<IReadOnlyList<UpcomingEpisodeDto>> GetNextEpisodesAsync(
        IReadOnlyList<string> seriesKeys, CancellationToken ct = default)
    {
        if (seriesKeys.Count == 0) return [];
        var response = await httpClient.PostAsJsonAsync("api/tv/next-episodes", seriesKeys, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<UpcomingEpisodeDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<SeasonDto?> GetSeasonAsync(string seriesKey, int seasonNumber, CancellationToken ct = default)
    {
        var (_, tmdbId) = TitleKey.Parse(seriesKey);
        var response = await httpClient.GetAsync($"api/titles/series/{tmdbId}/season/{seasonNumber}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SeasonDto>(cancellationToken: ct);
    }

    public async Task<IReadOnlyList<GenreDto>> GetGenresAsync(TitleType type, CancellationToken ct = default)
    {
        var segment = type == TitleType.Movie ? "movie" : "tv";
        return await httpClient.GetFromJsonAsync<List<GenreDto>>($"api/genres/{segment}", ct) ?? [];
    }
}
