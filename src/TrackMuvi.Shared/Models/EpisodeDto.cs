namespace TrackMuvi.Shared.Models;

/// <summary>Un episodio de TMDb. Sin estado personal (eso vive en SQLite del dispositivo).</summary>
public record EpisodeDto(
    int SeasonNumber,
    int EpisodeNumber,
    string Name,
    string? Overview,
    DateOnly? AirDate,
    int? RuntimeMinutes,
    string? StillPath);
