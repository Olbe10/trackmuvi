using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TrackMuvi.Api.Mapping;
using TrackMuvi.Api.Options;
using TrackMuvi.Api.Services;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Api.Controllers;

[ApiController]
[Route("api/movies")]
public class MoviesController(ITmdbClient tmdb, GenreCache genreCache, IOptions<TmdbOptions> options) : ControllerBase
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

    /// <summary>Películas en tendencia esta semana (carrusel "Próximos estrenos"/"Tendencias").</summary>
    [HttpGet("trending")]
    public async Task<ActionResult<IReadOnlyList<TitleSummaryDto>>> Trending(CancellationToken ct)
    {
        var response = await tmdb.GetTrendingMoviesAsync(ct);
        var genres = await genreCache.GetMovieGenresAsync(ct);
        return Ok(response.Results.Select(m => TitleMapper.MapMovieSummary(m, genres)).ToList());
    }

    /// <summary>
    /// "Muy pronto": películas ya confirmadas para un año futuro pero sin fecha de estreno
    /// específica todavía (ver TrackMuvi.Shared.Models.ReleaseDatePrecision). TMDb no tiene un
    /// filtro directo para esto, así que se recorren varias páginas de discover/movie por
    /// popularidad y se filtra el patrón de fecha placeholder; el resultado se confirma pidiendo
    /// el detalle (status distinto de "Released"/"Canceled") solo para esos candidatos, no para
    /// todo lo que se recorrió.
    /// </summary>
    [HttpGet("coming-soon")]
    public async Task<ActionResult<IReadOnlyList<TitleSummaryDto>>> ComingSoon(CancellationToken ct)
    {
        const int pagesToScan = 40;
        var from = DateOnly.FromDateTime(DateTime.Today).AddDays(1);

        var pages = await Task.WhenAll(Enumerable.Range(1, pagesToScan)
            .Select(page => tmdb.DiscoverFutureMoviesByPopularityAsync(from, page, ct)));

        var candidates = pages
            .SelectMany(r => r.Results)
            .Where(m => DateOnly.TryParse(m.ReleaseDate, out var d) && ReleaseDatePrecision.IsYearOnly(d))
            .DistinctBy(m => m.Id)
            .OrderByDescending(m => m.Popularity)
            .Take(60)
            .ToList();

        var details = await Task.WhenAll(candidates.Select(m => tmdb.GetMovieDetailAsync(m.Id, ct)));
        var genres = await genreCache.GetMovieGenresAsync(ct);

        var confirmed = candidates
            .Zip(details, (summary, detail) => (summary, detail))
            .Where(t => t.detail is { Status: "Planned" or "In Production" or "Post Production" })
            .OrderByDescending(t => t.summary.Popularity)
            .Select(t => TitleMapper.MapMovieSummary(t.summary, genres))
            .ToList();

        return Ok(confirmed);
    }
}
