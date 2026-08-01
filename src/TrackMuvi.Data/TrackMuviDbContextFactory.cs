using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TrackMuvi.Data;

/// <summary>
/// Solo para "dotnet ef migrations add" en tiempo de diseño (TrackMuvi.Data es una class
/// library sin Program.cs propio). En runtime, el dbPath real lo inyecta AddTrackMuviData.
/// </summary>
public class TrackMuviDbContextFactory : IDesignTimeDbContextFactory<TrackMuviDbContext>
{
    public TrackMuviDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TrackMuviDbContext>();
        optionsBuilder.UseSqlite("Data Source=design_time.db");
        return new TrackMuviDbContext(optionsBuilder.Options);
    }
}
