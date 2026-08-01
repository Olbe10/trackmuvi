using System.Text.Json.Serialization;

namespace TrackMuvi.Api.TmdbModels;

public class TmdbGenre
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}

public class TmdbGenreListResponse
{
    [JsonPropertyName("genres")] public List<TmdbGenre> Genres { get; set; } = [];
}

public class TmdbProductionCompany
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}

public class TmdbCollection
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}
