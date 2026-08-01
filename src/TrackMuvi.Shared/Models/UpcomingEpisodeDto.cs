namespace TrackMuvi.Shared.Models;

/// <summary>
/// Próximo episodio de una serie ("next_episode_to_air" de TMDb), usado para
/// avisar "nuevo episodio de una serie que sigo" y para el Calendario.
/// </summary>
public record UpcomingEpisodeDto(
    string SeriesKey,
    string SeriesTitle,
    string? SeriesPosterPath,
    int SeasonNumber,
    int EpisodeNumber,
    string EpisodeName,
    DateOnly AirDate);
