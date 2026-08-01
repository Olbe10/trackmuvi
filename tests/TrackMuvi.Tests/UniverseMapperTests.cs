using TrackMuvi.Api.Mapping;
using TrackMuvi.Api.TmdbModels;
using Xunit;

namespace TrackMuvi.Tests;

public class UniverseMapperTests
{
    [Fact]
    public void Resolve_ReturnsMarvel_WhenCompanyNameContainsMarvel()
    {
        var companies = new List<TmdbProductionCompany> { new() { Id = 420, Name = "Marvel Studios" } };
        Assert.Equal("Marvel", UniverseMapper.Resolve(companies, null));
    }

    [Fact]
    public void Resolve_ReturnsDc_WhenCollectionNameContainsDc()
    {
        var collection = new TmdbCollection { Id = 1, Name = "DC Extended Universe" };
        Assert.Equal("DC", UniverseMapper.Resolve([], collection));
    }

    [Fact]
    public void Resolve_ReturnsIndependiente_WhenNoMatch()
    {
        var companies = new List<TmdbProductionCompany> { new() { Id = 1, Name = "A24" } };
        Assert.Equal("Independiente", UniverseMapper.Resolve(companies, null));
    }
}
