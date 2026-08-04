namespace TrackMuvi.UI.State;

/// <summary>
/// Estado de navegación/filtros compartido entre páginas, equivalente al objeto `state`
/// del mockup (discCat, calMode, listTab, search, prevView). Se registra como singleton
/// en el host MAUI: como BlazorWebView no tiene "circuitos" independientes, un singleton
/// hace las veces de estado de sesión de la app.
/// </summary>
public class AppState
{
    public string DiscoverCategory { get; set; } = "all";
    public string CalendarFilter { get; set; } = "all";
    public string ListTab { get; set; } = "following";
    public string SearchQuery { get; set; } = string.Empty;

    /// <summary>Mes/día que se estaba viendo en Calendario, para no volver a "hoy" al entrar a una
    /// ficha de detalle y apretar atrás.</summary>
    public DateOnly? CalendarMonth { get; set; }
    public DateOnly? CalendarSelectedDate { get; set; }

    /// <summary>Ruta a la que debe volver el botón "atrás" de la ficha de detalle.</summary>
    public string PreviousRoute { get; set; } = "/";

    public readonly List<NotificationEntry> Notifications = [];

    public event Action? Changed;

    public void NotifyChanged() => Changed?.Invoke();
}

public record NotificationEntry(string Title, DateTimeOffset At);
