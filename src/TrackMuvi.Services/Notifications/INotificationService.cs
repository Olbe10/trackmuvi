namespace TrackMuvi.Services.Notifications;

/// <summary>
/// Notificaciones locales del dispositivo (sin backend push). Implementada en
/// TrackMuvi.Maui/Platforms/Android con Plugin.LocalNotification.
///
/// Los métodos Schedule* agendan la notificación para su fecha real de entrega (vía AlarmManager,
/// a través del plugin) en vez de dispararla en el momento: así llega aunque la app esté cerrada
/// o la API no responda ese día. Se llaman cada vez que ReleaseCheckService logra sincronizar con
/// la API; volver a agendar con el mismo titleKey reemplaza lo agendado antes para ese título.
/// </summary>
public interface INotificationService
{
    Task<bool> RequestPermissionAsync();

    /// <summary>Agenda "Título X se estrena hoy" para la fecha de estreno real de la película.</summary>
    Task ScheduleMovieReleaseAsync(string titleKey, string title, DateOnly releaseDate);

    /// <summary>Agenda "Nuevo episodio" para la fecha de emisión real del próximo episodio.</summary>
    Task ScheduleEpisodeAsync(
        string seriesKey, string seriesTitle, int seasonNumber, int episodeNumber, string episodeName, DateOnly airDate);

    /// <summary>"Cambió la fecha de estreno de X" — notificación inmediata (best effort: solo se
    /// entrega si el chequeo logra correr con la app viva/con red en el momento del cambio).</summary>
    Task NotifyDateChangedAsync(string titleKey, string title, DateOnly previousDate, DateOnly newDate);

    /// <summary>"X ya tiene fecha de estreno confirmada" — para cuando una película pasa de "solo
    /// se sabe el año" (ver ReleaseDatePrecision) a una fecha real. Notificación inmediata, distinta
    /// de NotifyDateChangedAsync porque antes no había una fecha "real" que haya cambiado.</summary>
    Task NotifyReleaseDateAnnouncedAsync(string titleKey, string title, DateOnly releaseDate);

    /// <summary>Notificación inmediata de prueba, para validar que el canal/los permisos funcionan.</summary>
    Task ShowTestNotificationAsync();

    /// <summary>Cancela cualquier notificación agendada para ese título (p. ej. al dejar de seguirlo).</summary>
    Task CancelAsync(string titleKey);
}
