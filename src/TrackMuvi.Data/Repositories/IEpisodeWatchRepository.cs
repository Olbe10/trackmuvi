namespace TrackMuvi.Data.Repositories;

public interface IEpisodeWatchRepository
{
    Task<HashSet<int>> GetWatchedEpisodeNumbersAsync(string titleKey, int seasonNumber, CancellationToken ct = default);

    /// <summary>Prende/apaga el flag de un episodio puntual. Devuelve el nuevo estado (true = visto).</summary>
    Task<bool> ToggleEpisodeWatchedAsync(string titleKey, int seasonNumber, int episodeNumber, CancellationToken ct = default);
}
