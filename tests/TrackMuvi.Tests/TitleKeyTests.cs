using TrackMuvi.Shared.Enums;
using TrackMuvi.Shared.Models;
using Xunit;

namespace TrackMuvi.Tests;

public class TitleKeyTests
{
    [Theory]
    [InlineData(TitleType.Movie, 603692, "movie-603692")]
    [InlineData(TitleType.Series, 1396, "series-1396")]
    public void Build_ProducesExpectedKey(TitleType type, int tmdbId, string expected)
    {
        Assert.Equal(expected, TitleKey.Build(type, tmdbId));
    }

    [Theory]
    [InlineData("movie-603692", TitleType.Movie, 603692)]
    [InlineData("series-1396", TitleType.Series, 1396)]
    public void Parse_RoundTripsBuild(string key, TitleType expectedType, int expectedId)
    {
        var (type, id) = TitleKey.Parse(key);
        Assert.Equal(expectedType, type);
        Assert.Equal(expectedId, id);
    }

    [Fact]
    public void Parse_ThrowsOnInvalidFormat()
    {
        Assert.Throws<FormatException>(() => TitleKey.Parse("not-a-valid-key-format"));
    }
}
