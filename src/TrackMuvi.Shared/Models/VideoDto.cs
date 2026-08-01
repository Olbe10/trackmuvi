namespace TrackMuvi.Shared.Models;

/// <summary>Un video de TMDb (/videos). Usamos los de Site == "YouTube" y Type == "Trailer".</summary>
public record VideoDto(
    string Key,
    string Name,
    string Site,
    string Type);
