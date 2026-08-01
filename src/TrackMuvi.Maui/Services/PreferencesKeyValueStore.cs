using TrackMuvi.Services.BackgroundSync;

namespace TrackMuvi.Maui.Services;

/// <summary>Microsoft.Maui.Storage.Preferences funciona igual en Android/iOS/Windows,
/// así que esta implementación no necesita vivir en Platforms/.</summary>
public class PreferencesKeyValueStore : IKeyValueStore
{
    public string? Get(string key) => Preferences.Default.Get<string?>(key, null);

    public void Set(string key, string value) => Preferences.Default.Set(key, value);
}
