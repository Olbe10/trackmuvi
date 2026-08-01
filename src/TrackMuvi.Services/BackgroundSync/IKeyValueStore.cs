namespace TrackMuvi.Services.BackgroundSync;

/// <summary>
/// Almacén clave/valor simple para el estado interno del chequeo de estrenos (última fecha
/// vista por título, para detectar cambios de fecha). Implementado en TrackMuvi.Maui con
/// Microsoft.Maui.Storage.Preferences.
/// </summary>
public interface IKeyValueStore
{
    string? Get(string key);
    void Set(string key, string value);
}
