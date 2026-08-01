using System.Text.Json.Serialization;

namespace TrackMuvi.Api.TmdbModels;

public class TmdbSeasonDetail
{
    [JsonPropertyName("season_number")] public int SeasonNumber { get; set; }
    [JsonPropertyName("episodes")] public List<TmdbEpisodeDetail> Episodes { get; set; } = [];
}

public class TmdbEpisodeDetail
{
    [JsonPropertyName("episode_number")] public int EpisodeNumber { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("overview")] public string? Overview { get; set; }
    [JsonPropertyName("air_date")] public string? AirDate { get; set; }
    [JsonPropertyName("runtime")] public int? Runtime { get; set; }
    [JsonPropertyName("still_path")] public string? StillPath { get; set; }
}
