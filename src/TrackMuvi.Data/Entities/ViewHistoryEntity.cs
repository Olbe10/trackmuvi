namespace TrackMuvi.Data.Entities;

/// <summary>Una marca de "vista" en el historial de un título. Puede haber varias por título.</summary>
public class ViewHistoryEntity
{
    public int Id { get; set; }
    public string TitleKey { get; set; } = string.Empty;
    public DateTimeOffset WatchedAt { get; set; }
}
