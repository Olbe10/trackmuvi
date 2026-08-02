using TrackMuvi.Shared.Enums;

namespace TrackMuvi.Shared.Models;

/// <summary>
/// Versión ligera de un título, usada en carruseles/listas (Home, Calendario, Descubrir, Mi Lista).
/// Equivale a los campos que el mockup lee de CATALOG para pintar trending-card / poster-card / row-card.
/// </summary>
public record TitleSummaryDto(
    string Key,
    int TmdbId,
    TitleType Type,
    string Title,
    string? Universe,
    string? PrimaryGenre,
    string? PrimaryPlatformLabel,
    DateOnly? ReleaseDate,
    int? Seasons,
    double Popularity,
    double VoteAverage,
    string? PosterPath,
    string? BackdropPath);
