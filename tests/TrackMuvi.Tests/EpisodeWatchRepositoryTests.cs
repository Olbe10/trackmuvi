using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TrackMuvi.Data;
using TrackMuvi.Data.Repositories;
using Xunit;

namespace TrackMuvi.Tests;

public class EpisodeWatchRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TrackMuviDbContext _db;
    private readonly EpisodeWatchRepository _repository;

    public EpisodeWatchRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TrackMuviDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new TrackMuviDbContext(options);
        _db.Database.EnsureCreated();
        _repository = new EpisodeWatchRepository(_db);
    }

    [Fact]
    public async Task ToggleEpisodeWatchedAsync_MarksWatched_WhenNotSetBefore()
    {
        var result = await _repository.ToggleEpisodeWatchedAsync("series-1", 2, 1);

        Assert.True(result);
    }

    [Fact]
    public async Task ToggleEpisodeWatchedAsync_UnmarksWatched_WhenToggledTwice()
    {
        await _repository.ToggleEpisodeWatchedAsync("series-1", 2, 1);
        var result = await _repository.ToggleEpisodeWatchedAsync("series-1", 2, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task GetWatchedEpisodeNumbersAsync_ScopesBySeasonAndTitle()
    {
        await _repository.ToggleEpisodeWatchedAsync("series-1", 2, 1);
        await _repository.ToggleEpisodeWatchedAsync("series-1", 2, 2);
        await _repository.ToggleEpisodeWatchedAsync("series-1", 3, 1); // otra temporada
        await _repository.ToggleEpisodeWatchedAsync("series-2", 2, 1); // otro título

        var watched = await _repository.GetWatchedEpisodeNumbersAsync("series-1", 2);

        Assert.Equal(new HashSet<int> { 1, 2 }, watched);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
