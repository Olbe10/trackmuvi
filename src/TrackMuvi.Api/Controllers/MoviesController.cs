using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TrackMuvi.Api.Mapping;
using TrackMuvi.Api.Options;
using TrackMuvi.Api.Services;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Api.Controllers;

[ApiController]
[Route("api/movies")]
public class MoviesController(ITmdbClient tmdb, GenreCache genreCache, IOptions<TmdbOptions> options, IMemoryCache cache) : ControllerBase
{
    /// <summary>
    /// Estrenos de cine en un rango de fechas (Calendario navega mes a mes con esto;
    /// Descubrir pide un rango amplio para su vista general). Si no se pasan from/to,
    /// por defecto es "desde hoy, los próximos 60 días".
    /// </summary>
    [HttpGet("upcoming")]
    public async Task<ActionResult<IReadOnlyList<TitleSummaryDto>>> Upcoming(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] int page, CancellationToken ct)
    {
        var effectiveFrom = from ?? DateOnly.FromDateTime(DateTime.Today);
        var effectiveTo = to ?? effectiveFrom.AddDays(60);

        var response = await tmdb.DiscoverMoviesByDateRangeAsync(effectiveFrom, effectiveTo, page < 1 ? 1 : page, ct);
        var genres = await genreCache.GetMovieGenresAsync(ct);
        var summaries = response.Results.Select(m => TitleMapper.MapMovieSummary(m, genres)).ToList();

        var region = options.Value.DefaultRegion;
        var withPlatforms = await Task.WhenAll(summaries.Select(async s =>
        {
            var providers = await tmdb.GetMovieWatchProvidersAsync(s.TmdbId, ct);
            var label = TitleMapper.ExtractPrimaryPlatformLabel(providers, region) ?? "Cine";
            return s with { PrimaryPlatformLabel = label };
        }));

        return Ok(withPlatforms);
    }

    /// <summary>Películas en tendencia esta semana (carrusel "Próximos estrenos"/"Tendencias").
    /// TMDb solo la actualiza semanalmente, así que se cachea 1h para no repetir la llamada en
    /// cada carga de Inicio/Descubrir.</summary>
    [HttpGet("trending")]
    public async Task<ActionResult<IReadOnlyList<TitleSummaryDto>>> Trending(CancellationToken ct)
    {
        var result = await cache.GetOrCreateAsync("movies:trending", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            var response = await tmdb.GetTrendingMoviesAsync(ct);
            var genres = await genreCache.GetMovieGenresAsync(ct);
            return response.Results.Select(m => TitleMapper.MapMovieSummary(m, genres)).ToList();
        });
        return Ok(result);
    }

    /// <summary>
    /// "Muy pronto": películas ya confirmadas para un año futuro pero sin fecha de estreno
    /// específica todavía (ver TrackMuvi.Shared.Models.ReleaseDatePrecision). TMDb no tiene un
    /// filtro directo para esto, así que se recorren varias páginas de discover/movie por
    /// popularidad y se filtra el patrón de fecha placeholder; el resultado se confirma pidiendo
    /// el detalle (status distinto de "Released"/"Canceled") solo para esos candidatos, no para
    /// todo lo que se recorrió.
    /// Se limita a original_language=en: mientras más profundo se escanea, más domina el resultado
    /// con producciones regionales de perfil muy bajo (a veces sin sinopsis siquiera), que para un
    /// usuario promedio son "rarezas" no una anticipada. Filtrar por inglés es un proxy simple de
    /// "conocida" dado que las grandes producciones no angloparlantes casi siempre ya tienen fecha
    /// confirmada para cuando alcanzan este nivel de popularidad.
    /// </summary>
    /// <summary>Top en críticas (nota de TMDb), catálogo paginado para la pestaña Descubrir.
    /// Se cachea 6h por página: el ranking por vote_average casi no se mueve de un día a otro.</summary>
    [HttpGet("top-rated")]
    public async Task<ActionResult<IReadOnlyList<TitleSummaryDto>>> TopRated([FromQuery] int page, CancellationToken ct)
    {
        var effectivePage = page < 1 ? 1 : page;
        var result = await cache.GetOrCreateAsync($"movies:top-rated:{effectivePage}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);
            var response = await tmdb.DiscoverTopRatedMoviesAsync(effectivePage, ct);
            var genres = await genreCache.GetMovieGenresAsync(ct);
            return response.Results.Select(m => TitleMapper.MapMovieSummary(m, genres)).ToList();
        });
        return Ok(result);
    }

    [HttpGet("coming-soon")]
    public async Task<ActionResult<IReadOnlyList<TitleSummaryDto>>> ComingSoon(CancellationToken ct)
    {
        // El escaneo de 80 páginas (~95 llamadas a TMDb entre discover + verificación de status)
        // es el endpoint más caro de toda la API; esto casi no cambia de una hora a otra, así que
        // se cachea 6h en vez de repetirlo en cada carga de Descubrir.
        var result = await cache.GetOrCreateAsync("movies:coming-soon", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);

            const int pagesToScan = 80;
            var from = DateOnly.FromDateTime(DateTime.Today).AddDays(1);

            var pages = await Task.WhenAll(Enumerable.Range(1, pagesToScan)
                .Select(page => tmdb.DiscoverFutureMoviesByPopularityAsync(from, page, ct)));

            var candidates = pages
                .SelectMany(r => r.Results)
                .Where(m => m.OriginalLanguage == "en")
                .Where(m => DateOnly.TryParse(m.ReleaseDate, out var d) && ReleaseDatePrecision.IsYearOnly(d))
                .DistinctBy(m => m.Id)
                .OrderByDescending(m => m.Popularity)
                .Take(60)
                .ToList();

            // Solo se necesita el status acá (no cast/videos/watch-providers), así que se usa el
            // detalle liviano en vez de GetMovieDetailAsync para no pagar esos appends x60.
            var details = await Task.WhenAll(candidates.Select(m => tmdb.GetMovieBasicAsync(m.Id, ct)));
            var genres = await genreCache.GetMovieGenresAsync(ct);

            return candidates
                .Zip(details, (summary, detail) => (summary, detail))
                .Where(t => t.detail is { Status: "Planned" or "In Production" or "Post Production" })
                .OrderByDescending(t => t.summary.Popularity)
                .Select(t => TitleMapper.MapMovieSummary(t.summary, genres))
                .ToList();
        });

        return Ok(result);
    }
}
