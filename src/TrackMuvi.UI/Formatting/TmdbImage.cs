namespace TrackMuvi.UI.Formatting;

/// <summary>
/// El CDN de imágenes de TMDb es público (no requiere API key), así que la UI arma las
/// URLs directamente en vez de pasar por TrackMuvi.Api.
/// </summary>
public static class TmdbImage
{
    private const string BaseUrl = "https://image.tmdb.org/t/p";

    public static string? Poster(string? path, string size = "w342") => Build(path, size);
    public static string? Backdrop(string? path, string size = "w780") => Build(path, size);
    public static string? Profile(string? path, string size = "w185") => Build(path, size);

    private static string? Build(string? path, string size) =>
        string.IsNullOrWhiteSpace(path) ? null : $"{BaseUrl}/{size}{path}";
}
