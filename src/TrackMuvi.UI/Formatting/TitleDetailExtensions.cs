using TrackMuvi.Shared.Models;

namespace TrackMuvi.UI.Formatting;

public static class TitleDetailExtensions
{
    /// <summary>Reduce una ficha completa a la versión liviana que usan las tarjetas de lista.</summary>
    public static TitleSummaryDto ToSummary(this TitleDetailDto d) => new(
        d.Key, d.TmdbId, d.Type, d.Title, d.Universe,
        d.Genres.FirstOrDefault()?.Name,
        d.PrimaryPlatformLabel,
        d.ReleaseDate, d.Seasons, d.Popularity, d.VoteAverage, d.PosterPath, d.BackdropPath);
}
