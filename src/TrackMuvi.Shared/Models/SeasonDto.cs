namespace TrackMuvi.Shared.Models;

public record SeasonDto(int SeasonNumber, IReadOnlyList<EpisodeDto> Episodes);
