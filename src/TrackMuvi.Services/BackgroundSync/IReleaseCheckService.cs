using TrackMuvi.Shared.Models;

namespace TrackMuvi.Services.BackgroundSync;

/// <summary>
/// Revisa estrenos de mañana, nuevos episodios y cambios de fecha para los títulos que el
/// usuario tiene en "Quiero ver"/"Siguiendo", y dispara notificaciones locales. Se llama al
/// abrir la app (MVP); no requiere un servicio en segundo plano del SO todavía.
/// </summary>
public interface IReleaseCheckService
{
    Task RunCheckAsync(CancellationToken ct = default);

    /// <summary>Corre RunCheckAsync y devuelve lo que cae exactamente hoy, para mostrar un aviso
    /// dentro de la app (Inicio) como red de seguridad si la notificación push no llegó a tiempo
    /// (p. ej. la app no se abrió entre el episodio anterior y hoy, ver ReleaseCheckService).</summary>
    Task<IReadOnlyList<TodayReleaseDto>> GetTodayReleasesAsync(CancellationToken ct = default);
}
