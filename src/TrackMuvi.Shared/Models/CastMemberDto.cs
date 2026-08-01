namespace TrackMuvi.Shared.Models;

public record CastMemberDto(
    int PersonId,
    string Name,
    string? Character,
    string? ProfilePath);
