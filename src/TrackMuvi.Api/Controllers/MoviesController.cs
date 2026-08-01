using Microsoft.AspNetCore.Mvc;
using TrackMuvi.Api.Mapping;
using TrackMuvi.Api.Services;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Api.Controllers;

[ApiController]
[Route("api/movies")]
public class MoviesController(ITmdbClient tmdb, GenreCache genreCache) : ControllerBase
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
        return Ok(response.Results.Select(m => TitleMapper.MapMovieSummary(m, genres)).ToList());
    }

    /// <summary>Películas en tendencia esta semana (carrusel "Próximos estrenos"/"Tendencias").</summary>
    [HttpGet("trending")]
    public async Task<ActionResult<IReadOnlyList<TitleSummaryDto>>> Trending(CancellationToken ct)
    {
        var response = await tmdb.GetTrendingMoviesAsync(ct);
        var genres = await genreCache.GetMovieGenresAsync(ct);
        return Ok(response.Results.Select(m => TitleMapper.MapMovieSummary(m, genres)).ToList());
    }
}
