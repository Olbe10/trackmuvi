namespace TrackMuvi.Shared.Models;

/// <summary>
/// TMDb no tiene un campo de "solo se sabe el año de estreno": cuando una película está
/// confirmada para un año pero todavía no tiene fecha exacta, la convención (de TMDb y de sus
/// editores) es guardar el 1 de enero de ese año como placeholder. Este helper identifica ese
/// patrón para no tratarlo como una fecha de estreno real (ni mostrarla, ni agendar una
/// notificación de "se estrena hoy" para un 1 de enero que no es real).
/// </summary>
public static class ReleaseDatePrecision
{
    public static bool IsYearOnly(DateOnly date) => date is { Month: 1, Day: 1 };
}
