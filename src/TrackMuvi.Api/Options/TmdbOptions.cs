namespace TrackMuvi.Api.Options;

/// <summary>
/// Configuración de TMDb. AccessToken NUNCA va en appsettings.json versionado:
/// en desarrollo se setea con "dotnet user-secrets", en Azure con un App Setting
/// (Tmdb__AccessToken). Ver README para instrucciones.
/// </summary>
public class TmdbOptions
{
    public const string SectionName = "Tmdb";

    // OJO: debe terminar en "/" para que HttpClient.BaseAddress combine bien con rutas relativas.
    public string BaseUrl { get; set; } = "https://api.themoviedb.org/3/";
    public string ImageBaseUrl { get; set; } = "https://image.tmdb.org/t/p";
    public string AccessToken { get; set; } = string.Empty;
    public string DefaultLanguage { get; set; } = "es-ES";
    public string DefaultRegion { get; set; } = "US";
}
