using Microsoft.AspNetCore.Mvc;
using TrackMuvi.Api.Mapping;
using TrackMuvi.Api.Services;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Api.Controllers;

[ApiController]
[Route("api/movies")]
public class MoviesController(ITmdbClient tmdb, GenreCache genreCache) : ControllerBase
{
    /// <summary>Próximos estrenos de cine (Calendario / Inicio).</summary>
    [HttpGet("upcoming")]
    public async Task<ActionResult<IReadOnlyList<TitleSummaryDto>>> Upcoming(
        [FromQuery] int page, CancellationToken ct)
    {
        var response = await tmdb.GetUpcomingMoviesAsync(page < 1 ? 1 : page, ct);
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
