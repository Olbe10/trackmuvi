using TrackMuvi.Shared.Enums;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.UI.Formatting;

public static class UpcomingEpisodeExtensions
{
    /// <summary>Para poder pintar un próximo episodio con los mismos RowCard/PosterCard que un título.</summary>
    public static TitleSummaryDto ToSummary(this UpcomingEpisodeDto ep) => new(
        ep.SeriesKey,
        TitleKey.Parse(ep.SeriesKey).TmdbId,
        TitleType.Series,
        $"{ep.SeriesTitle} — T{ep.SeasonNumber}E{ep.EpisodeNumber}",
        Universe: null,
        PrimaryGenre: null,
        PrimaryPlatformLabel: null,
        ReleaseDate: ep.AirDate,
        Seasons: ep.SeasonNumber,
        Popularity: 0,
        PosterPath: ep.SeriesPosterPath,
        BackdropPath: null);
}
