using Microsoft.AspNetCore.Mvc;
using TrackMuvi.Api.Mapping;
using TrackMuvi.Api.Services;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Api.Controllers;

[ApiController]
[Route("api/tv")]
public class TvController(ITmdbClient tmdb, GenreCache genreCache) : ControllerBase
{
    /// <summary>Series en tendencia esta semana.</summary>
    [HttpGet("trending")]
    public async Task<ActionResult<IReadOnlyList<TitleSummaryDto>>> Trending(CancellationToken ct)
    {
        var response = await tmdb.GetTrendingTvAsync(ct);
        var genres = await genreCache.GetTvGenresAsync(ct);
        return Ok(response.Results.Select(t => TitleMapper.MapTvSummary(t, genres)).ToList());
    }

    /// <summary>
    /// Próximo episodio de cada serie que el usuario sigue (estado personal vive en SQLite del
    /// dispositivo, así que el cliente nos manda las keys y nosotros resolvemos next_episode_to_air).
    /// </summary>
    [HttpPost("next-episodes")]
    public async Task<ActionResult<IReadOnlyList<UpcomingEpisodeDto>>> NextEpisodes(
        [FromBody] List<string> seriesKeys, CancellationToken ct)
    {
        var tmdbIds = seriesKeys
            .Select(key => TitleKey.Parse(key))
            .Where(parsed => parsed.Type == Shared.Enums.TitleType.Series)
            .Select(parsed => parsed.TmdbId)
            .Distinct();

        var details = await Task.WhenAll(tmdbIds.Select(id => tmdb.GetTvDetailAsync(id, ct)));

        var episodes = details
            .Where(d => d is not null)
            .Select(d => TitleMapper.MapNextEpisode(d!))
            .Where(ep => ep is not null)
            .Select(ep => ep!)
            .OrderBy(ep => ep.AirDate)
            .ToList();

        return Ok(episodes);
    }
}
