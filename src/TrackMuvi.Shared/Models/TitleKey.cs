using TrackMuvi.Shared.Enums;

namespace TrackMuvi.Shared.Models;

/// <summary>
/// Identificador estable de un título a través de API, SQLite y navegación en la UI.
/// Formato: "movie-603692" / "series-1396" (tipo + id de TMDb).
/// </summary>
public static class TitleKey
{
    public static string Build(TitleType type, int tmdbId) =>
        $"{(type == TitleType.Movie ? "movie" : "series")}-{tmdbId}";

    public static (TitleType Type, int TmdbId) Parse(string key)
    {
        var parts = key.Split('-', 2);
        if (parts.Length != 2 || !int.TryParse(parts[1], out var id))
        {
            throw new FormatException($"'{key}' no es un TitleKey válido (esperado 'movie-123' o 'series-123').");
        }

        var type = parts[0] switch
        {
            "movie" => TitleType.Movie,
            "series" => TitleType.Series,
            _ => throw new FormatException($"Tipo de título desconocido en '{key}'.")
        };

        return (type, id);
    }
}
