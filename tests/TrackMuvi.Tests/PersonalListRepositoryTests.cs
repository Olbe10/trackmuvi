using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TrackMuvi.Data;
using TrackMuvi.Data.Repositories;
using TrackMuvi.Shared.Enums;
using Xunit;

namespace TrackMuvi.Tests;

public class PersonalListRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TrackMuviDbContext _db;
    private readonly PersonalListRepository _repository;

    public PersonalListRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TrackMuviDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new TrackMuviDbContext(options);
        _db.Database.EnsureCreated();
        _repository = new PersonalListRepository(_db);
    }

    [Fact]
    public async Task ToggleFlagAsync_TurnsFlagOn_WhenNotSetBefore()
    {
        var result = await _repository.ToggleFlagAsync("movie-1", PersonalStatusFlag.Want);

        Assert.True(result.Want);
        Assert.False(result.Watched);
    }

    [Fact]
    public async Task ToggleFlagAsync_TurnsFlagOff_WhenToggledTwice()
    {
        await _repository.ToggleFlagAsync("movie-1", PersonalStatusFlag.Following);
        var result = await _repository.ToggleFlagAsync("movie-1", PersonalStatusFlag.Following);

        Assert.False(result.Following);
    }

    [Fact]
    public async Task GetKeysByFlagAsync_ReturnsOnlyMatchingTitles()
    {
        await _repository.ToggleFlagAsync("movie-1", PersonalStatusFlag.Favorite);
        await _repository.ToggleFlagAsync("movie-2", PersonalStatusFlag.Want);

        var favorites = await _repository.GetKeysByFlagAsync(PersonalStatusFlag.Favorite);

        Assert.Single(favorites);
        Assert.Equal("movie-1", favorites[0]);
    }

    [Fact]
    public async Task AddViewHistoryEntryAsync_IsRetrievableByTitleKey()
    {
        var now = DateTimeOffset.UtcNow;
        await _repository.AddViewHistoryEntryAsync("movie-1", now);

        var history = await _repository.GetViewHistoryAsync("movie-1");

        Assert.Single(history);
        Assert.Equal("movie-1", history[0].TitleKey);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
