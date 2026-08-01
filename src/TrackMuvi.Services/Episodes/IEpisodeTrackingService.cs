namespace TrackMuvi.Services.Episodes;

/// <summary>Combina el catálogo de episodios de TMDb con el estado "visto" local (SQLite).</summary>
public interface IEpisodeTrackingService
{
    Task<SeasonProgressDto?> GetSeasonAsync(string seriesKey, int seasonNumber, CancellationToken ct = default);

    /// <summary>Prende/apaga el flag de un episodio. Devuelve el nuevo estado (true = visto).</summary>
    Task<bool> ToggleEpisodeWatchedAsync(string seriesKey, int seasonNumber, int episodeNumber, CancellationToken ct = default);
}
