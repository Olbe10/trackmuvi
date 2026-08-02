using System.Text.Json.Serialization;

namespace TrackMuvi.Api.TmdbModels;

public class TmdbMovieListResponse
{
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("results")] public List<TmdbMovieSummary> Results { get; set; } = [];
    [JsonPropertyName("total_pages")] public int TotalPages { get; set; }
    [JsonPropertyName("total_results")] public int TotalResults { get; set; }
}

public class TmdbMovieSummary
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("overview")] public string? Overview { get; set; }
    [JsonPropertyName("poster_path")] public string? PosterPath { get; set; }
    [JsonPropertyName("backdrop_path")] public string? BackdropPath { get; set; }
    [JsonPropertyName("release_date")] public string? ReleaseDate { get; set; }
    [JsonPropertyName("popularity")] public double Popularity { get; set; }
    [JsonPropertyName("genre_ids")] public List<int> GenreIds { get; set; } = [];
    [JsonPropertyName("original_language")] public string? OriginalLanguage { get; set; }
}

public class TmdbMovieDetail
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("overview")] public string? Overview { get; set; }
    [JsonPropertyName("poster_path")] public string? PosterPath { get; set; }
    [JsonPropertyName("backdrop_path")] public string? BackdropPath { get; set; }
    [JsonPropertyName("release_date")] public string? ReleaseDate { get; set; }
    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("runtime")] public int? Runtime { get; set; }
    [JsonPropertyName("popularity")] public double Popularity { get; set; }
    [JsonPropertyName("genres")] public List<TmdbGenre> Genres { get; set; } = [];
    [JsonPropertyName("production_companies")] public List<TmdbProductionCompany> ProductionCompanies { get; set; } = [];
    [JsonPropertyName("belongs_to_collection")] public TmdbCollection? BelongsToCollection { get; set; }
    [JsonPropertyName("credits")] public TmdbCredits? Credits { get; set; }
    [JsonPropertyName("videos")] public TmdbVideosResponse? Videos { get; set; }
    [JsonPropertyName("watch/providers")] public TmdbWatchProvidersResponse? WatchProviders { get; set; }
    [JsonPropertyName("release_dates")] public TmdbReleaseDatesResponse? ReleaseDates { get; set; }
    [JsonPropertyName("images")] public TmdbImagesResponse? Images { get; set; }
}
