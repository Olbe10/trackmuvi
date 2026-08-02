using System.Text.Json.Serialization;

namespace TrackMuvi.Api.TmdbModels;

public class TmdbSearchMultiResponse
{
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("results")] public List<TmdbMultiSearchItem> Results { get; set; } = [];
    [JsonPropertyName("total_pages")] public int TotalPages { get; set; }
    [JsonPropertyName("total_results")] public int TotalResults { get; set; }
}

/// <summary>Item de /search/multi: puede ser movie, tv o person (filtramos "person" al mapear).</summary>
public class TmdbMultiSearchItem
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("media_type")] public string MediaType { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("poster_path")] public string? PosterPath { get; set; }
    [JsonPropertyName("backdrop_path")] public string? BackdropPath { get; set; }
    [JsonPropertyName("release_date")] public string? ReleaseDate { get; set; }
    [JsonPropertyName("first_air_date")] public string? FirstAirDate { get; set; }
    [JsonPropertyName("popularity")] public double Popularity { get; set; }
    [JsonPropertyName("vote_average")] public double VoteAverage { get; set; }
    [JsonPropertyName("genre_ids")] public List<int> GenreIds { get; set; } = [];
}
