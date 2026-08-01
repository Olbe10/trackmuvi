namespace TrackMuvi.Shared.Models;

/// <summary>Estado personal de un título, tal como se guarda en SQLite (TrackMuvi.Data).</summary>
public record PersonalStatusDto(
    string TitleKey,
    bool Want,
    bool Watched,
    bool Favorite,
    bool Following,
    bool Pending,
    bool Abandoned,
    DateTimeOffset UpdatedAt)
{
    public static PersonalStatusDto Empty(string titleKey) =>
        new(titleKey, false, false, false, false, false, false, DateTimeOffset.MinValue);
}
