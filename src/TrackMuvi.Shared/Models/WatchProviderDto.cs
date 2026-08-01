namespace TrackMuvi.Shared.Models;

/// <summary>Un "dónde verla" de TMDb (/watch/providers), ej. Netflix en modo flatrate.</summary>
public record WatchProviderDto(
    int ProviderId,
    string ProviderName,
    string? LogoPath,
    WatchProviderType Type);

public enum WatchProviderType
{
    Flatrate,
    Rent,
    Buy,
    Free,
    Ads
}
