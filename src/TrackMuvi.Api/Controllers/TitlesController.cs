using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TrackMuvi.Api.Mapping;
using TrackMuvi.Api.Options;
using TrackMuvi.Api.Services;
using TrackMuvi.Shared.Enums;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Api.Controllers;

[ApiController]
[Route("api/titles")]
public class TitlesController(ITmdbClient tmdb, GenreCache genreCache, IOptions<TmdbOptions> options, IMemoryCache cache) : ControllerBase
{
    /// <summary>
    /// Ficha de un título. Con full=true (default, pantalla Detail): poster, sinopsis, reparto,
    /// tráiler, dónde verla (credits/videos/watch-providers/images/certificación via
    /// append_to_response). Con full=false: solo los campos base de TMDb (título, poster, fecha,
    /// status, género, universo, duración) para listas/resúmenes que no necesitan lo demás — evita
    /// pagar esos appends en Inicio/Mi Lista/Perfil/notificaciones, que solo leen esos campos base.
    /// </summary>
    [HttpGet("{type}/{tmdbId:int}")]
    public async Task<ActionResult<TitleDetailDto>> Get(
        string type, int tmdbId, [FromQuery] bool full = true, CancellationToken ct = default)
    {
        var region = options.Value.DefaultRegion;

        if (type.Equals("movie", StringComparison.OrdinalIgnoreCase))
        {
            var detail = full ? await tmdb.GetMovieDetailAsync(tmdbId, ct) : await tmdb.GetMovieBasicAsync(tmdbId, ct);
            return detail is null ? NotFound() : Ok(TitleMapper.MapMovieDetail(detail, region));
        }

        if (type.Equals("series", StringComparison.OrdinalIgnoreCase) || type.Equals("tv", StringComparison.OrdinalIgnoreCase))
        {
            var detail = full ? await tmdb.GetTvDetailAsync(tmdbId, ct) : await tmdb.GetTvBasicAsync(tmdbId, ct);
            return detail is null ? NotFound() : Ok(TitleMapper.MapTvDetail(detail, region));
        }

        return BadRequest($"Tipo '{type}' inválido. Usa 'movie' o 'series'.");
    }

    /// <summary>Igual que arriba pero recibiendo el TitleKey compuesto (ej. "movie-603692").</summary>
    [HttpGet("by-key/{titleKey}")]
    public Task<ActionResult<TitleDetailDto>> GetByKey(string titleKey, [FromQuery] bool full = true, CancellationToken ct = default)
    {
        var (type, tmdbId) = TitleKey.Parse(titleKey);
        return Get(type == TitleType.Movie ? "movie" : "series", tmdbId, full, ct);
    }

    /// <summary>
    /// "Descubre algo nuevo" (Inicio): recomendaciones de TMDb ("porque viste/marcaste X") a partir
    /// de un puñado de títulos base que manda el cliente (sus favoritas/vistas más recientes — Mi
    /// Lista vive en SQLite local, no en esta API). Se agregan todas, se sacan los propios títulos
    /// base y duplicados, y se ordena por popularidad.
    /// </summary>
    [HttpPost("recommendations")]
    public async Task<ActionResult<IReadOnlyList<TitleSummaryDto>>> Recommendations(
        [FromBody] List<string> basisTitleKeys, CancellationToken ct)
    {
        if (basisTitleKeys.Count == 0) return Ok(Array.Empty<TitleSummaryDto>());

        var cacheKey = "titles:recommendations:" + string.Join(',', basisTitleKeys.OrderBy(k => k));
        var result = await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);

            var movieGenres = await genreCache.GetMovieGenresAsync(ct);
            var tvGenres = await genreCache.GetTvGenresAsync(ct);

            var perBasis = await Task.WhenAll(basisTitleKeys.Select(async key =>
            {
                var (type, tmdbId) = TitleKey.Parse(key);
                if (type == TitleType.Movie)
                {
                    var response = await tmdb.GetMovieRecommendationsAsync(tmdbId, ct);
                    return response.Results
                        .Where(m => m.OriginalLanguage == "en")
                        .Select(m => TitleMapper.MapMovieSummary(m, movieGenres));
                }

                var tvResponse = await tmdb.GetTvRecommendationsAsync(tmdbId, ct);
                return tvResponse.Results
                    .Where(t => t.OriginalLanguage == "en")
                    .Select(t => TitleMapper.MapTvSummary(t, tvGenres));
            }));

            var basisKeys = basisTitleKeys.ToHashSet();
            return perBasis
                .SelectMany(r => r)
                .Where(t => !basisKeys.Contains(t.Key))
                .DistinctBy(t => t.Key)
                .OrderByDescending(t => t.Popularity)
                .Take(20)
                .ToList();
        });

        return Ok(result);
    }

    /// <summary>Episodios de una temporada (pantalla Detail de series, tab de episodios).</summary>
    [HttpGet("series/{tmdbId:int}/season/{seasonNumber:int}")]
    public async Task<ActionResult<SeasonDto>> GetSeason(int tmdbId, int seasonNumber, CancellationToken ct)
    {
        var detail = await tmdb.GetSeasonDetailAsync(tmdbId, seasonNumber, ct);
        return detail is null ? NotFound() : Ok(TitleMapper.MapSeason(detail));
    }
}
