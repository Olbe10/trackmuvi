using Plugin.LocalNotification;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Maui.Services;

/// <summary>Notificaciones locales de Android vía Plugin.LocalNotification (sin backend push).</summary>
public class AndroidNotificationService : TrackMuvi.Services.Notifications.INotificationService
{
    public Task<bool> RequestPermissionAsync() =>
        LocalNotificationCenter.Current.RequestNotificationPermission();

    public Task ScheduleReleaseTomorrowAsync(TitleDetailDto title) => Show(
        title.Key,
        "¡Se estrena mañana!",
        $"{title.Title} se estrena mañana.");

    public Task ScheduleNewEpisodeAsync(UpcomingEpisodeDto episode) => Show(
        $"{episode.SeriesKey}-s{episode.SeasonNumber}e{episode.EpisodeNumber}",
        "Nuevo episodio",
        $"{episode.SeriesTitle} T{episode.SeasonNumber}E{episode.EpisodeNumber} \"{episode.EpisodeName}\" sale el {episode.AirDate:d 'de' MMMM}.");

    public Task ScheduleDateChangedAsync(TitleDetailDto title, DateOnly previousDate, DateOnly newDate) => Show(
        title.Key,
        "Cambió la fecha de estreno",
        $"{title.Title} pasó de estrenarse el {previousDate:d 'de' MMMM} al {newDate:d 'de' MMMM}.");

    public Task ShowTestNotificationAsync() => Show(
        "test",
        "TrackMuvi",
        "Las notificaciones locales están funcionando.");

    public Task CancelAsync(string titleKey)
    {
        LocalNotificationCenter.Current.Cancel(NotificationIdFor(titleKey));
        return Task.CompletedTask;
    }

    private static Task Show(string idSource, string title, string description) =>
        LocalNotificationCenter.Current.Show(new NotificationRequest
        {
            NotificationId = NotificationIdFor(idSource),
            Title = title,
            Description = description,
        });

    private static int NotificationIdFor(string source) => Math.Abs(source.GetHashCode() % 1_000_000);
}
