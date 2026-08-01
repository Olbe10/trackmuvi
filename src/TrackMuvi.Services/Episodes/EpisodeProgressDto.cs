namespace TrackMuvi.Services.Episodes;

/// <summary>
/// Un episodio combinado con el estado personal (visto/no visto, vive en SQLite del
/// dispositivo). No es un DTO de TrackMuvi.Api: se arma acá, en el cliente.
/// </summary>
public record EpisodeProgressDto(
    int SeasonNumber,
    int EpisodeNumber,
    string Name,
    string? Overview,
    DateOnly? AirDate,
    int? RuntimeMinutes,
    string? StillPath,
    bool Watched,
    bool HasAired);

public record SeasonProgressDto(
    int SeasonNumber,
    IReadOnlyList<EpisodeProgressDto> Episodes,
    int AiredCount,
    int WatchedCount);
