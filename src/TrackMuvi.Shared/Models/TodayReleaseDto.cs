using TrackMuvi.Shared.Enums;

namespace TrackMuvi.Shared.Models;

/// <summary>
/// Algo de lo que el usuario sigue/quiere ver que se estrena (película) o emite (episodio) hoy,
/// según el último snapshot local (TitleCache). Se usa para mostrar un aviso dentro de la app como
/// red de seguridad cuando la notificación push no llegó a tiempo (ver ReleaseCheckService).
/// </summary>
public record TodayReleaseDto(
    string Key,
    TitleType Type,
    string Title,
    string? PosterPath,
    int? SeasonNumber,
    int? EpisodeNumber,
    string? EpisodeName);
