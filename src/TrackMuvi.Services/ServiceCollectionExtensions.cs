using Microsoft.Extensions.DependencyInjection;
using TrackMuvi.Services.Api;
using TrackMuvi.Services.BackgroundSync;
using TrackMuvi.Services.Episodes;
using TrackMuvi.Services.Personal;

namespace TrackMuvi.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra el cliente HTTP hacia TrackMuvi.Api y los servicios de negocio del cliente.
    /// No registra INotificationService/ICalendarSyncService/IKeyValueStore: esas son
    /// implementaciones específicas de plataforma que registra el proyecto host (TrackMuvi.Maui).
    /// </summary>
    public static IServiceCollection AddTrackMuviServices(this IServiceCollection services, string apiBaseUrl)
    {
        services.AddHttpClient<ITrackMuviApiClient, TrackMuviApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        });

        services.AddScoped<IPersonalListService, PersonalListService>();
        services.AddScoped<IReleaseCheckService, ReleaseCheckService>();
        services.AddScoped<IEpisodeTrackingService, EpisodeTrackingService>();
        return services;
    }
}
