using Microsoft.AspNetCore.Mvc;
using TrackMuvi.Api.Mapping;
using TrackMuvi.Api.Services;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Api.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController(ITmdbClient tmdb, GenreCache genreCache) : ControllerBase
{
    /// <summary>Búsqueda de películas/series por título (pantalla Descubrir).</summary>
    [HttpGet]
    public async Task<ActionResult<SearchResultDto>> Search(
        [FromQuery] string query, [FromQuery] int page, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query)) return Ok(new SearchResultDto([], 1, 0, 0));

        var effectivePage = page < 1 ? 1 : page;
        var response = await tmdb.SearchMultiAsync(query, effectivePage, ct);
        var movieGenres = await genreCache.GetMovieGenresAsync(ct);
        var tvGenres = await genreCache.GetTvGenresAsync(ct);

        var results = response.Results
            .Select(item => TitleMapper.MapSearchItem(item, movieGenres, tvGenres))
            .Where(dto => dto is not null)
            .Select(dto => dto!)
            .ToList();

        return Ok(new SearchResultDto(results, response.Page, response.TotalPages, response.TotalResults));
    }
}
