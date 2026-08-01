using System.Globalization;

namespace TrackMuvi.UI.Formatting;

/// <summary>Equivalentes a daysUntil/fmtDate/monthKey del mockup original (locale es).</summary>
public static class DateFormatting
{
    private static readonly CultureInfo Es = CultureInfo.GetCultureInfo("es-ES");

    public static int DaysUntil(DateOnly date) =>
        date.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;

    public static string RelativeDays(DateOnly date)
    {
        var d = DaysUntil(date);
        return d < 0 ? "Disponible" : d == 0 ? "¡Hoy!" : $"en {d}d";
    }

    public static string FullDate(DateOnly date) =>
        date.ToDateTime(TimeOnly.MinValue).ToString("d 'de' MMMM 'de' yyyy", Es);

    public static string MonthFlag(DateOnly date) =>
        date.ToDateTime(TimeOnly.MinValue).ToString("MMMM", Es).ToUpperInvariant();

    public static string MonthYearLabel(DateOnly date) =>
        date.ToDateTime(TimeOnly.MinValue).ToString("MMMM yyyy", Es);
}
