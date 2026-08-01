namespace TrackMuvi.Services.Episodes;

/// <summary>Combina el catálogo de episodios de TMDb con el estado "visto" local (SQLite).</summary>
public interface IEpisodeTrackingService
{
    Task<SeasonProgressDto?> GetSeasonAsync(string seriesKey, int seasonNumber, CancellationToken ct = default);

    /// <summary>Prende/apaga el flag de un episodio. Devuelve el nuevo estado (true = visto).</summary>
    Task<bool> ToggleEpisodeWatchedAsync(string seriesKey, int seasonNumber, int episodeNumber, CancellationToken ct = default);

    /// <summary>Marca (o desmarca) todos los episodios ya emitidos de la temporada de una sola vez.</summary>
    Task<SeasonProgressDto?> SetSeasonWatchedAsync(string seriesKey, int seasonNumber, bool watched, CancellationToken ct = default);
}
