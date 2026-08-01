namespace TrackMuvi.Shared.Models;

/// <summary>Una entrada del historial de vistas (punto 2 del MVP: historial por título).</summary>
public record ViewHistoryEntryDto(string TitleKey, DateTimeOffset WatchedAt);
