using Plugin.LocalNotification;

namespace TrackMuvi.Maui.Services;

/// <summary>Notificaciones locales de Android vía Plugin.LocalNotification (sin backend push).</summary>
public class AndroidNotificationService : TrackMuvi.Services.Notifications.INotificationService
{
    private const int ReleaseHourLocal = 9;

    public Task<bool> RequestPermissionAsync() =>
        LocalNotificationCenter.Current.RequestNotificationPermission();

    public Task ScheduleMovieReleaseAsync(string titleKey, string title, DateOnly releaseDate) => Schedule(
        titleKey,
        "¡Se estrena hoy!",
        $"{title} se estrena hoy.",
        NotifyTimeFor(releaseDate));

    public Task ScheduleEpisodeAsync(
        string seriesKey, string seriesTitle, int seasonNumber, int episodeNumber, string episodeName, DateOnly airDate) => Schedule(
        $"{seriesKey}-s{seasonNumber}e{episodeNumber}",
        "Nuevo episodio",
        $"{seriesTitle} T{seasonNumber}E{episodeNumber} \"{episodeName}\" sale hoy.",
        NotifyTimeFor(airDate));

    public Task NotifyDateChangedAsync(string titleKey, string title, DateOnly previousDate, DateOnly newDate) => Show(
        titleKey,
        "Cambió la fecha de estreno",
        $"{title} pasó de estrenarse el {previousDate:d 'de' MMMM} al {newDate:d 'de' MMMM}.");

    public Task NotifyReleaseDateAnnouncedAsync(string titleKey, string title, DateOnly releaseDate) => Show(
        titleKey,
        "¡Ya tiene fecha de estreno!",
        $"{title} se estrena el {releaseDate:d 'de' MMMM 'de' yyyy}.");

    public Task ShowTestNotificationAsync() => Show(
        "test",
        "TrackMuvi",
        "Las notificaciones locales están funcionando.");

    public Task CancelAsync(string titleKey)
    {
        LocalNotificationCenter.Current.Cancel(NotificationIdFor(titleKey));
        return Task.CompletedTask;
    }

    /// <summary>Hora local de entrega para una fecha de estreno. Si ya pasó (p. ej. la primera
    /// sincronización ocurre el mismo día del estreno, después de las 9am), se entrega ya mismo
    /// en vez de agendarse "en el pasado".</summary>
    private static DateTime NotifyTimeFor(DateOnly date)
    {
        var notifyTime = date.ToDateTime(new TimeOnly(ReleaseHourLocal, 0));
        return notifyTime <= DateTime.Now ? DateTime.Now.AddSeconds(5) : notifyTime;
    }

    /// <summary>
    /// Notificación agendada a futuro: Plugin.LocalNotification la entrega vía el AlarmManager de
    /// Android, así que llega aunque la app esté cerrada o no haya red en ese momento — lo único
    /// que necesita red es este llamado, que ocurre cuando ReleaseCheckService logra sincronizar
    /// con la API. AlarmType.RtcWakeup (default del plugin) despierta el dispositivo si hace falta.
    /// </summary>
    private static Task Schedule(string idSource, string title, string description, DateTime notifyTime) =>
        LocalNotificationCenter.Current.Show(new NotificationRequest
        {
            NotificationId = NotificationIdFor(idSource),
            Title = title,
            Description = description,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = notifyTime,
            },
        });

    private static Task Show(string idSource, string title, string description) =>
        LocalNotificationCenter.Current.Show(new NotificationRequest
        {
            NotificationId = NotificationIdFor(idSource),
            Title = title,
            Description = description,
        });

    private static int NotificationIdFor(string source) => Math.Abs(source.GetHashCode() % 1_000_000);
}
