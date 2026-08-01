using TrackMuvi.Shared.Models;

namespace TrackMuvi.Services.Notifications;

/// <summary>
/// Notificaciones locales del dispositivo (sin backend push). Implementada en
/// TrackMuvi.Maui/Platforms/Android con Plugin.LocalNotification.
/// </summary>
public interface INotificationService
{
    Task<bool> RequestPermissionAsync();

    /// <summary>"Título X se estrena mañana" — se agenda cuando faltan ~24h para el release.</summary>
    Task ScheduleReleaseTomorrowAsync(TitleDetailDto title);

    /// <summary>"Nuevo episodio de una serie que sigues".</summary>
    Task ScheduleNewEpisodeAsync(UpcomingEpisodeDto episode);

    /// <summary>"Cambió la fecha de estreno de X".</summary>
    Task ScheduleDateChangedAsync(TitleDetailDto title, DateOnly previousDate, DateOnly newDate);

    /// <summary>Notificación inmediata de prueba, para validar que el canal/los permisos funcionan.</summary>
    Task ShowTestNotificationAsync();

    Task CancelAsync(string titleKey);
}
