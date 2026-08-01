namespace TrackMuvi.Data.Repositories;

public interface IEpisodeWatchRepository
{
    Task<HashSet<int>> GetWatchedEpisodeNumbersAsync(string titleKey, int seasonNumber, CancellationToken ct = default);

    /// <summary>Prende/apaga el flag de un episodio puntual. Devuelve el nuevo estado (true = visto).</summary>
    Task<bool> ToggleEpisodeWatchedAsync(string titleKey, int seasonNumber, int episodeNumber, CancellationToken ct = default);

    /// <summary>Marca (o desmarca) de una vez todos los episodios indicados de una temporada.</summary>
    Task SetEpisodesWatchedAsync(string titleKey, int seasonNumber, IEnumerable<int> episodeNumbers, bool watched, CancellationToken ct = default);
}
