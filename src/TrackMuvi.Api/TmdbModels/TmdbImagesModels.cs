using System.Text.Json.Serialization;

namespace TrackMuvi.Api.TmdbModels;

public class TmdbImagesResponse
{
    [JsonPropertyName("backdrops")] public List<TmdbImageItem> Backdrops { get; set; } = [];
}

public class TmdbImageItem
{
    [JsonPropertyName("file_path")] public string FilePath { get; set; } = string.Empty;
}
