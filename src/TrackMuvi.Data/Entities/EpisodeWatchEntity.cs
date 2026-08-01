namespace TrackMuvi.Data.Entities;

/// <summary>Marca de "visto" a nivel episodio (independiente del flag Watched de todo el título).</summary>
public class EpisodeWatchEntity
{
    public string TitleKey { get; set; } = string.Empty;
    public int SeasonNumber { get; set; }
    public int EpisodeNumber { get; set; }
    public DateTimeOffset WatchedAt { get; set; }
}
