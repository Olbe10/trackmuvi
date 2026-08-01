namespace TrackMuvi.Shared.Models;

public record SearchResultDto(
    IReadOnlyList<TitleSummaryDto> Results,
    int Page,
    int TotalPages,
    int TotalResults);
