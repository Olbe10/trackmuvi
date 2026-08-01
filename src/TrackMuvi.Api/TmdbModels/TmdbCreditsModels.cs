using System.Text.Json.Serialization;

namespace TrackMuvi.Api.TmdbModels;

public class TmdbCredits
{
    [JsonPropertyName("cast")] public List<TmdbCastMember> Cast { get; set; } = [];
    [JsonPropertyName("crew")] public List<TmdbCrewMember> Crew { get; set; } = [];
}

public class TmdbCastMember
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("character")] public string? Character { get; set; }
    [JsonPropertyName("profile_path")] public string? ProfilePath { get; set; }
    [JsonPropertyName("order")] public int Order { get; set; }
}

public class TmdbCrewMember
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("job")] public string Job { get; set; } = string.Empty;
    [JsonPropertyName("department")] public string Department { get; set; } = string.Empty;
}
