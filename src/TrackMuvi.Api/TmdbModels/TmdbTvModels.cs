using System.Text.Json.Serialization;

namespace TrackMuvi.Api.TmdbModels;

public class TmdbTvListResponse
{
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("results")] public List<TmdbTvSummary> Results { get; set; } = [];
    [JsonPropertyName("total_pages")] public int TotalPages { get; set; }
    [JsonPropertyName("total_results")] public int TotalResults { get; set; }
}

public class TmdbTvSummary
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("overview")] public string? Overview { get; set; }
    [JsonPropertyName("poster_path")] public string? PosterPath { get; set; }
    [JsonPropertyName("backdrop_path")] public string? BackdropPath { get; set; }
    [JsonPropertyName("first_air_date")] public string? FirstAirDate { get; set; }
    [JsonPropertyName("popularity")] public double Popularity { get; set; }
    [JsonPropertyName("genre_ids")] public List<int> GenreIds { get; set; } = [];
}

public class TmdbTvDetail
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("overview")] public string? Overview { get; set; }
    [JsonPropertyName("poster_path")] public string? PosterPath { get; set; }
    [JsonPropertyName("backdrop_path")] public string? BackdropPath { get; set; }
    [JsonPropertyName("first_air_date")] public string? FirstAirDate { get; set; }
    [JsonPropertyName("popularity")] public double Popularity { get; set; }
    [JsonPropertyName("number_of_seasons")] public int? NumberOfSeasons { get; set; }
    [JsonPropertyName("genres")] public List<TmdbGenre> Genres { get; set; } = [];
    [JsonPropertyName("production_companies")] public List<TmdbProductionCompany> ProductionCompanies { get; set; } = [];
    [JsonPropertyName("created_by")] public List<TmdbCreator> CreatedBy { get; set; } = [];
    [JsonPropertyName("credits")] public TmdbCredits? Credits { get; set; }
    [JsonPropertyName("videos")] public TmdbVideosResponse? Videos { get; set; }
    [JsonPropertyName("watch/providers")] public TmdbWatchProvidersResponse? WatchProviders { get; set; }
    [JsonPropertyName("content_ratings")] public TmdbContentRatingsResponse? ContentRatings { get; set; }
    [JsonPropertyName("next_episode_to_air")] public TmdbEpisode? NextEpisodeToAir { get; set; }
    [JsonPropertyName("images")] public TmdbImagesResponse? Images { get; set; }
}

public class TmdbCreator
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}

public class TmdbEpisode
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("season_number")] public int SeasonNumber { get; set; }
    [JsonPropertyName("episode_number")] public int EpisodeNumber { get; set; }
    [JsonPropertyName("air_date")] public string? AirDate { get; set; }
}
