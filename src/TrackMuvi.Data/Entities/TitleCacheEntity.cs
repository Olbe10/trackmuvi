using TrackMuvi.Shared.Enums;

namespace TrackMuvi.Data.Entities;

/// <summary>
/// Último snapshot conocido (título, poster, fecha de estreno/próximo episodio) de cada título
/// marcado Following/Want. Se actualiza cada vez que ReleaseCheckService logra hablar con la API,
/// y es lo que permite agendar/mostrar notificaciones de estreno aunque ese día la API esté caída
/// o dormida (Render free tier) o la app esté cerrada.
/// </summary>
public class TitleCacheEntity
{
    public string TitleKey { get; set; } = string.Empty;
    public TitleType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? PosterPath { get; set; }

    /// <summary>Solo películas.</summary>
    public DateOnly? ReleaseDate { get; set; }

    /// <summary>Solo series: próximo episodio a emitirse.</summary>
    public int? NextEpisodeSeason { get; set; }
    public int? NextEpisodeNumber { get; set; }
    public string? NextEpisodeName { get; set; }
    public DateOnly? NextEpisodeAirDate { get; set; }

    public DateTimeOffset LastSyncedAt { get; set; }
}
