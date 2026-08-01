using Android.Content;
using Android.Database;
using Android.Provider;
using TrackMuvi.Services.Calendar;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Maui.Services;

/// <summary>
/// Sincroniza estrenos con el Calendario nativo de Android (Calendar Provider). Cada evento
/// lleva un marcador "[trackmuvi:{titleKey}]" en la descripción para poder actualizarlo o
/// borrarlo después en vez de duplicarlo.
/// </summary>
public class AndroidCalendarSyncService : ICalendarSyncService
{
    private static ContentResolver Resolver => Android.App.Application.Context.ContentResolver!;

    public async Task<bool> RequestPermissionAsync()
    {
        var write = await Permissions.RequestAsync<Permissions.CalendarWrite>();
        var read = await Permissions.RequestAsync<Permissions.CalendarRead>();
        return write == PermissionStatus.Granted && read == PermissionStatus.Granted;
    }

    public Task AddOrUpdateEventAsync(TitleDetailDto title)
    {
        if (title.ReleaseDate is not { } releaseDate) return Task.CompletedTask;

        var calendarId = GetWritableCalendarId();
        if (calendarId is null) return Task.CompletedTask;

        var marker = Marker(title.Key);
        var startUtcMillis = new DateTimeOffset(releaseDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
            .ToUnixTimeMilliseconds();
        var endUtcMillis = startUtcMillis + TimeSpan.FromDays(1).Milliseconds;

        var values = new ContentValues();
        values.Put(CalendarContract.Events.InterfaceConsts.CalendarId, calendarId.Value);
        values.Put(CalendarContract.Events.InterfaceConsts.Title, $"Estreno: {title.Title}");
        values.Put(CalendarContract.Events.InterfaceConsts.Description, $"TrackMuvi — {title.Synopsis}\n\n{marker}");
        values.Put(CalendarContract.Events.InterfaceConsts.Dtstart, startUtcMillis);
        values.Put(CalendarContract.Events.InterfaceConsts.Dtend, endUtcMillis);
        values.Put(CalendarContract.Events.InterfaceConsts.AllDay, 1);
        values.Put(CalendarContract.Events.InterfaceConsts.EventTimezone, Java.Util.TimeZone.Default!.ID);

        var existingId = FindEventId(marker);
        if (existingId is { } id)
        {
            Resolver.Update(ContentUris.WithAppendedId(CalendarContract.Events.ContentUri!, id), values, null, null);
        }
        else
        {
            Resolver.Insert(CalendarContract.Events.ContentUri!, values);
        }

        return Task.CompletedTask;
    }

    public Task RemoveEventAsync(string titleKey)
    {
        var existingId = FindEventId(Marker(titleKey));
        if (existingId is { } id)
        {
            Resolver.Delete(ContentUris.WithAppendedId(CalendarContract.Events.ContentUri!, id), null, null);
        }

        return Task.CompletedTask;
    }

    private static string Marker(string titleKey) => $"[trackmuvi:{titleKey}]";

    private static long? GetWritableCalendarId()
    {
        string[] projection = [CalendarContract.Calendars.InterfaceConsts.Id, CalendarContract.Calendars.InterfaceConsts.CalendarAccessLevel];
        using ICursor? cursor = Resolver.Query(CalendarContract.Calendars.ContentUri!, projection, null, null, null);
        if (cursor is null) return null;

        long? bestId = null;
        var bestAccess = -1;
        while (cursor.MoveToNext())
        {
            var id = cursor.GetLong(0);
            var access = cursor.GetInt(1);
            if (access > bestAccess)
            {
                bestAccess = access;
                bestId = id;
            }
        }

        return bestId;
    }

    private static long? FindEventId(string marker)
    {
        string[] projection = [CalendarContract.Events.InterfaceConsts.Id, CalendarContract.Events.InterfaceConsts.Description];
        using ICursor? cursor = Resolver.Query(CalendarContract.Events.ContentUri!, projection, null, null, null);
        if (cursor is null) return null;

        while (cursor.MoveToNext())
        {
            var description = cursor.GetString(1);
            if (description is not null && description.Contains(marker))
            {
                return cursor.GetLong(0);
            }
        }

        return null;
    }
}
