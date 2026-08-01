using TrackMuvi.Shared.Models;

namespace TrackMuvi.Services.Calendar;

/// <summary>
/// Sincronización con el Calendario nativo del dispositivo (Android Calendar Provider).
/// Implementada en TrackMuvi.Maui/Platforms/Android. Sin OAuth ni Google/Outlook/Apple Calendar
/// por ahora — solo el calendario local del dispositivo (fuera de alcance del MVP).
/// </summary>
public interface ICalendarSyncService
{
    Task<bool> RequestPermissionAsync();

    /// <summary>Crea o actualiza el evento de estreno de este título en el calendario del dispositivo.</summary>
    Task AddOrUpdateEventAsync(TitleDetailDto title);

    /// <summary>Elimina el evento de calendario asociado a este título (ej. al dejar de seguirlo).</summary>
    Task RemoveEventAsync(string titleKey);
}
