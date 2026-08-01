using Microsoft.AspNetCore.Mvc;
using TrackMuvi.Api.Services;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Api.Controllers;

[ApiController]
[Route("api/genres")]
public class GenresController(GenreCache genreCache) : ControllerBase
{
    /// <summary>Géneros para los chips de categoría de Descubrir.</summary>
    [HttpGet("{type}")]
    public async Task<ActionResult<IReadOnlyList<GenreDto>>> Get(string type, CancellationToken ct)
    {
        var lookup = type.Equals("movie", StringComparison.OrdinalIgnoreCase)
            ? await genreCache.GetMovieGenresAsync(ct)
            : await genreCache.GetTvGenresAsync(ct);

        return Ok(lookup.Select(kv => new GenreDto(kv.Key, kv.Value)).OrderBy(g => g.Name).ToList());
    }
}
