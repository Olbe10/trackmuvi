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

    /// <summary>Episodios de una temporada (pantalla Detail de series, tab de episodios).</summary>
    [HttpGet("series/{tmdbId:int}/season/{seasonNumber:int}")]
    public async Task<ActionResult<SeasonDto>> GetSeason(int tmdbId, int seasonNumber, CancellationToken ct)
    {
        var detail = await tmdb.GetSeasonDetailAsync(tmdbId, seasonNumber, ct);
        return detail is null ? NotFound() : Ok(TitleMapper.MapSeason(detail));
    }
}
