using System.Text.Json.Serialization;

namespace TrackMuvi.Api.TmdbModels;

public class TmdbVideosResponse
{
    [JsonPropertyName("results")] public List<TmdbVideo> Results { get; set; } = [];
}

public class TmdbVideo
{
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("site")] public string Site { get; set; } = string.Empty;
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("official")] public bool Official { get; set; }
}
