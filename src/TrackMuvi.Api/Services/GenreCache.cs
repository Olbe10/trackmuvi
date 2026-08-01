using Microsoft.Extensions.Caching.Memory;

namespace TrackMuvi.Api.Services;

/// <summary>
/// Los géneros de TMDb casi no cambian; los cacheamos en memoria para no pedirlos
/// en cada búsqueda/listado (evita duplicar llamadas por cada item de una lista).
/// </summary>
public class GenreCache(ITmdbClient tmdbClient, IMemoryCache cache)
{
    private const string MovieKey = "genres:movie";
    private const string TvKey = "genres:tv";
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(12);

    public async Task<IReadOnlyDictionary<int, string>> GetMovieGenresAsync(CancellationToken ct) =>
        await cache.GetOrCreateAsync(MovieKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = Ttl;
            var genres = await tmdbClient.GetMovieGenresAsync(ct);
            return (IReadOnlyDictionary<int, string>)genres.ToDictionary(g => g.Id, g => g.Name);
        }) ?? new Dictionary<int, string>();

    public async Task<IReadOnlyDictionary<int, string>> GetTvGenresAsync(CancellationToken ct) =>
        await cache.GetOrCreateAsync(TvKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = Ttl;
            var genres = await tmdbClient.GetTvGenresAsync(ct);
            return (IReadOnlyDictionary<int, string>)genres.ToDictionary(g => g.Id, g => g.Name);
        }) ?? new Dictionary<int, string>();
}
