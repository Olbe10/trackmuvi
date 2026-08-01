namespace TrackMuvi.Services.BackgroundSync;

/// <summary>
/// Revisa estrenos de mañana, nuevos episodios y cambios de fecha para los títulos que el
/// usuario tiene en "Quiero ver"/"Siguiendo", y dispara notificaciones locales. Se llama al
/// abrir la app (MVP); no requiere un servicio en segundo plano del SO todavía.
/// </summary>
public interface IReleaseCheckService
{
    Task RunCheckAsync(CancellationToken ct = default);
}
