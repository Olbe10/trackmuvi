using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TrackMuvi.Api.Mapping;
using TrackMuvi.Api.Options;
using TrackMuvi.Api.Services;
using TrackMuvi.Shared.Enums;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Api.Controllers;

[ApiController]
[Route("api/titles")]
public class TitlesController(ITmdbClient tmdb, IOptions<TmdbOptions> options) : ControllerBase
{
    /// <summary>Ficha completa (pantalla Detail): poster, sinopsis, reparto, tráiler, dónde verla.</summary>
    [HttpGet("{type}/{tmdbId:int}")]
    public async Task<ActionResult<TitleDetailDto>> Get(string type, int tmdbId, CancellationToken ct)
    {
        var region = options.Value.DefaultRegion;

        if (type.Equals("movie", StringComparison.OrdinalIgnoreCase))
        {
            var detail = await tmdb.GetMovieDetailAsync(tmdbId, ct);
            return detail is null ? NotFound() : Ok(TitleMapper.MapMovieDetail(detail, region));
        }

        if (type.Equals("series", StringComparison.OrdinalIgnoreCase) || type.Equals("tv", StringComparison.OrdinalIgnoreCase))
        {
            var detail = await tmdb.GetTvDetailAsync(tmdbId, ct);
            return detail is null ? NotFound() : Ok(TitleMapper.MapTvDetail(detail, region));
        }

        return BadRequest($"Tipo '{type}' inválido. Usa 'movie' o 'series'.");
    }

    /// <summary>Igual que arriba pero recibiendo el TitleKey compuesto (ej. "movie-603692").</summary>
    [HttpGet("by-key/{titleKey}")]
    public Task<ActionResult<TitleDetailDto>> GetByKey(string titleKey, CancellationToken ct)
    {
        var (type, tmdbId) = TitleKey.Parse(titleKey);
        return Get(type == TitleType.Movie ? "movie" : "series", tmdbId, ct);
    }
}
