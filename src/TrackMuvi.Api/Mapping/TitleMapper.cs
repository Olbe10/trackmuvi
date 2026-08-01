using TrackMuvi.Api.TmdbModels;
using TrackMuvi.Shared.Enums;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Api.Mapping;

public static class TitleMapper
{
    public static TitleSummaryDto MapMovieSummary(TmdbMovieSummary m, IReadOnlyDictionary<int, string> movieGenres)
    {
        var key = TitleKey.Build(TitleType.Movie, m.Id);
        return new TitleSummaryDto(
            key, m.Id, TitleType.Movie, m.Title,
            Universe: null,
            PrimaryGenre: m.GenreIds.Select(id => movieGenres.GetValueOrDefault(id)).FirstOrDefault(g => g is not null),
            PrimaryPlatformLabel: "Cine",
            ReleaseDate: ParseDate(m.ReleaseDate),
            Seasons: null,
            Popularity: m.Popularity,
            PosterPath: m.PosterPath,
            BackdropPath: m.BackdropPath);
    }

    public static TitleSummaryDto MapTvSummary(TmdbTvSummary t, IReadOnlyDictionary<int, string> tvGenres)
    {
        var key = TitleKey.Build(TitleType.Series, t.Id);
        return new TitleSummaryDto(
            key, t.Id, TitleType.Series, t.Name,
            Universe: null,
            PrimaryGenre: t.GenreIds.Select(id => tvGenres.GetValueOrDefault(id)).FirstOrDefault(g => g is not null),
            PrimaryPlatformLabel: null,
            ReleaseDate: ParseDate(t.FirstAirDate),
            Seasons: null,
            Popularity: t.Popularity,
            PosterPath: t.PosterPath,
            BackdropPath: t.BackdropPath);
    }

    public static TitleSummaryDto? MapSearchItem(
        TmdbMultiSearchItem item,
        IReadOnlyDictionary<int, string> movieGenres,
        IReadOnlyDictionary<int, string> tvGenres)
    {
        switch (item.MediaType)
        {
            case "movie":
                return new TitleSummaryDto(
                    TitleKey.Build(TitleType.Movie, item.Id), item.Id, TitleType.Movie, item.Title ?? "(sin título)",
                    null,
                    item.GenreIds.Select(id => movieGenres.GetValueOrDefault(id)).FirstOrDefault(g => g is not null),
                    "Cine",
                    ParseDate(item.ReleaseDate), null, item.Popularity, item.PosterPath, item.BackdropPath);
            case "tv":
                return new TitleSummaryDto(
                    TitleKey.Build(TitleType.Series, item.Id), item.Id, TitleType.Series, item.Name ?? "(sin título)",
                    null,
                    item.GenreIds.Select(id => tvGenres.GetValueOrDefault(id)).FirstOrDefault(g => g is not null),
                    null,
                    ParseDate(item.FirstAirDate), null, item.Popularity, item.PosterPath, item.BackdropPath);
            default:
                return null; // "person" u otros media_type se ignoran
        }
    }

    public static TitleDetailDto MapMovieDetail(TmdbMovieDetail d, string region)
    {
        var key = TitleKey.Build(TitleType.Movie, d.Id);
        var directors = d.Credits?.Crew
            .Where(c => c.Job == "Director")
            .Select(c => c.Name)
            .Distinct()
            .ToList() ?? [];

        var watchProviders = ExtractWatchProviders(d.WatchProviders, region);

        return new TitleDetailDto(
            key, d.Id, TitleType.Movie, d.Title,
            Universe: UniverseMapper.Resolve(d.ProductionCompanies, d.BelongsToCollection),
            Genres: d.Genres.Select(g => new GenreDto(g.Id, g.Name)).ToList(),
            // Si ya está disponible en streaming/alquiler/compra, mostramos esa plataforma real;
            // si no hay ninguna todavía (recién estrenada o solo en cines), cae a "Cine".
            PrimaryPlatformLabel: watchProviders.FirstOrDefault()?.ProviderName ?? "Cine",
            ReleaseDate: ParseDate(d.ReleaseDate),
            Seasons: null,
            DurationMinutes: d.Runtime,
            Rating: ExtractMovieCertification(d.ReleaseDates, region),
            Popularity: d.Popularity,
            Synopsis: d.Overview ?? string.Empty,
            Creators: directors,
            Cast: MapCast(d.Credits),
            PosterPath: d.PosterPath,
            BackdropPath: d.BackdropPath,
            TrailerYoutubeKey: ExtractTrailerKey(d.Videos),
            WatchProviders: watchProviders,
            GalleryBackdropPaths: ExtractGallery(d.Images));
    }

    public static TitleDetailDto MapTvDetail(TmdbTvDetail d, string region)
    {
        var key = TitleKey.Build(TitleType.Series, d.Id);
        return new TitleDetailDto(
            key, d.Id, TitleType.Series, d.Name,
            Universe: UniverseMapper.Resolve(d.ProductionCompanies, null),
            Genres: d.Genres.Select(g => new GenreDto(g.Id, g.Name)).ToList(),
            PrimaryPlatformLabel: ExtractWatchProviders(d.WatchProviders, region).FirstOrDefault()?.ProviderName,
            ReleaseDate: ParseDate(d.FirstAirDate),
            Seasons: d.NumberOfSeasons,
            DurationMinutes: null,
            Rating: ExtractTvCertification(d.ContentRatings, region),
            Popularity: d.Popularity,
            Synopsis: d.Overview ?? string.Empty,
            Creators: d.CreatedBy.Select(c => c.Name).ToList(),
            Cast: MapCast(d.Credits),
            PosterPath: d.PosterPath,
            BackdropPath: d.BackdropPath,
            TrailerYoutubeKey: ExtractTrailerKey(d.Videos),
            WatchProviders: ExtractWatchProviders(d.WatchProviders, region),
            GalleryBackdropPaths: ExtractGallery(d.Images));
    }

    public static UpcomingEpisodeDto? MapNextEpisode(TmdbTvDetail d)
    {
        if (d.NextEpisodeToAir is not { } ep) return null;
        var airDate = ParseDate(ep.AirDate);
        if (airDate is null) return null;

        return new UpcomingEpisodeDto(
            TitleKey.Build(TitleType.Series, d.Id),
            d.Name,
            d.PosterPath,
            ep.SeasonNumber,
            ep.EpisodeNumber,
            ep.Name,
            airDate.Value);
    }

    public static SeasonDto MapSeason(TmdbSeasonDetail d) => new(
        d.SeasonNumber,
        d.Episodes.Select(e => new EpisodeDto(
            d.SeasonNumber, e.EpisodeNumber, e.Name, e.Overview, ParseDate(e.AirDate), e.Runtime, e.StillPath)).ToList());

    private static List<CastMemberDto> MapCast(TmdbCredits? credits) =>
        credits?.Cast
            .OrderBy(c => c.Order)
            .Take(12)
            .Select(c => new CastMemberDto(c.Id, c.Name, c.Character, c.ProfilePath))
            .ToList() ?? [];

    private static string? ExtractTrailerKey(TmdbVideosResponse? videos)
    {
        if (videos is null) return null;
        var youtubeVideos = videos.Results.Where(v => v.Site == "YouTube").ToList();
        return youtubeVideos.FirstOrDefault(v => v.Type == "Trailer" && v.Official)?.Key
            ?? youtubeVideos.FirstOrDefault(v => v.Type == "Trailer")?.Key
            ?? youtubeVideos.FirstOrDefault()?.Key;
    }

    private static string? ExtractMovieCertification(TmdbReleaseDatesResponse? releaseDates, string region)
    {
        var entry = releaseDates?.Results.FirstOrDefault(r => r.CountryCode == region);
        return entry?.ReleaseDates.Select(r => r.Certification).FirstOrDefault(c => !string.IsNullOrWhiteSpace(c));
    }

    private static string? ExtractTvCertification(TmdbContentRatingsResponse? contentRatings, string region)
    {
        var entry = contentRatings?.Results.FirstOrDefault(r => r.CountryCode == region);
        return string.IsNullOrWhiteSpace(entry?.Rating) ? null : entry.Rating;
    }

    private static List<WatchProviderDto> ExtractWatchProviders(TmdbWatchProvidersResponse? providers, string region)
    {
        if (providers is null || !providers.Results.TryGetValue(region, out var country)) return [];

        return Flatten(country.Flatrate, WatchProviderType.Flatrate)
            .Concat(Flatten(country.Free, WatchProviderType.Free))
            .Concat(Flatten(country.Ads, WatchProviderType.Ads))
            .Concat(Flatten(country.Rent, WatchProviderType.Rent))
            .Concat(Flatten(country.Buy, WatchProviderType.Buy))
            .ToList();

        static IEnumerable<WatchProviderDto> Flatten(List<TmdbProvider>? source, WatchProviderType type) =>
            source?.Select(p => new WatchProviderDto(p.ProviderId, p.ProviderName, p.LogoPath, type)) ?? [];
    }

    private static List<string> ExtractGallery(TmdbImagesResponse? images) =>
        images?.Backdrops.Take(3).Select(b => b.FilePath).ToList() ?? [];

    private static DateOnly? ParseDate(string? raw) =>
        !string.IsNullOrWhiteSpace(raw) && DateOnly.TryParse(raw, out var date) ? date : null;
}
