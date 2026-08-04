using System.Text.Json.Serialization;

namespace TrackMuvi.Api.TmdbModels;

public class TmdbReleaseDatesResponse
{
    [JsonPropertyName("results")] public List<TmdbCountryReleaseDates> Results { get; set; } = [];
}

public class TmdbCountryReleaseDates
{
    [JsonPropertyName("iso_3166_1")] public string CountryCode { get; set; } = string.Empty;
    [JsonPropertyName("release_dates")] public List<TmdbReleaseDateEntry> ReleaseDates { get; set; } = [];
}

public class TmdbReleaseDateEntry
{
    [JsonPropertyName("certification")] public string Certification { get; set; } = string.Empty;
    [JsonPropertyName("type")] public int Type { get; set; }
    [JsonPropertyName("release_date")] public string? ReleaseDate { get; set; }
}

public class TmdbContentRatingsResponse
{
    [JsonPropertyName("results")] public List<TmdbCountryContentRating> Results { get; set; } = [];
}

public class TmdbCountryContentRating
{
    [JsonPropertyName("iso_3166_1")] public string CountryCode { get; set; } = string.Empty;
    [JsonPropertyName("rating")] public string Rating { get; set; } = string.Empty;
}
