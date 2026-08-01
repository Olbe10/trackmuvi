using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrackMuvi.Data.Repositories;

namespace TrackMuvi.Data;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra el DbContext de SQLite y los repositorios. El dbPath lo decide el host
    /// (en MAUI: FileSystem.AppDataDirectory) porque TrackMuvi.Data no conoce del sistema
    /// de archivos de la plataforma.
    /// </summary>
    public static IServiceCollection AddTrackMuviData(this IServiceCollection services, string dbPath)
    {
        services.AddDbContext<TrackMuviDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
        services.AddScoped<IPersonalListRepository, PersonalListRepository>();
        services.AddScoped<IEpisodeWatchRepository, EpisodeWatchRepository>();
        services.AddScoped<ITitleCacheRepository, TitleCacheRepository>();
        return services;
    }
}
