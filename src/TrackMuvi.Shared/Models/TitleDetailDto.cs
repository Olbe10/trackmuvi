using TrackMuvi.Shared.Enums;

namespace TrackMuvi.Shared.Models;

/// <summary>
/// Ficha completa de un título (pantalla Detail del mockup). Combina
/// /movie|tv/{id} + append_to_response=credits,videos,watch/providers,release_dates|content_ratings.
/// </summary>
public record TitleDetailDto(
    string Key,
    int TmdbId,
    TitleType Type,
    string Title,
    string? Universe,
    IReadOnlyList<GenreDto> Genres,
    string? PrimaryPlatformLabel,
    DateOnly? ReleaseDate,
    int? Seasons,
    int? DurationMinutes,
    string? Rating,
    double Popularity,
    double VoteAverage,
    string? Status,
    string Synopsis,
    IReadOnlyList<string> Creators,
    IReadOnlyList<CastMemberDto> Cast,
    string? PosterPath,
    string? BackdropPath,
    string? TrailerYoutubeKey,
    IReadOnlyList<WatchProviderDto> WatchProviders,
    IReadOnlyList<string> GalleryBackdropPaths);
