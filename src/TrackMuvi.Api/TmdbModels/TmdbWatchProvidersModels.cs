using System.Text.Json.Serialization;

namespace TrackMuvi.Api.TmdbModels;

public class TmdbWatchProvidersResponse
{
    [JsonPropertyName("results")] public Dictionary<string, TmdbCountryProviders> Results { get; set; } = new();
}

public class TmdbCountryProviders
{
    [JsonPropertyName("flatrate")] public List<TmdbProvider>? Flatrate { get; set; }
    [JsonPropertyName("rent")] public List<TmdbProvider>? Rent { get; set; }
    [JsonPropertyName("buy")] public List<TmdbProvider>? Buy { get; set; }
    [JsonPropertyName("free")] public List<TmdbProvider>? Free { get; set; }
    [JsonPropertyName("ads")] public List<TmdbProvider>? Ads { get; set; }
}

public class TmdbProvider
{
    [JsonPropertyName("provider_id")] public int ProviderId { get; set; }
    [JsonPropertyName("provider_name")] public string ProviderName { get; set; } = string.Empty;
    [JsonPropertyName("logo_path")] public string? LogoPath { get; set; }
}
