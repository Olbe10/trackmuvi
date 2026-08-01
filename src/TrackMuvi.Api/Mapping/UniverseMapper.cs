using TrackMuvi.Api.TmdbModels;

namespace TrackMuvi.Api.Mapping;

/// <summary>
/// TMDb no tiene un campo "universo" nativo. Lo aproximamos por nombre de productora/colección,
/// igual que en el mockup (Marvel / DC / Independiente). Es una heurística cosmética para el
/// badge de la ficha de detalle, no una taxonomía exacta.
/// </summary>
public static class UniverseMapper
{
    public static string Resolve(IEnumerable<TmdbProductionCompany> companies, TmdbCollection? collection)
    {
        var names = companies.Select(c => c.Name).ToList();
        if (collection is not null) names.Add(collection.Name);

        if (names.Any(n => n.Contains("Marvel", StringComparison.OrdinalIgnoreCase)))
            return "Marvel";

        if (names.Any(n => n.Contains("DC ", StringComparison.OrdinalIgnoreCase)
                            || n.Equals("DC", StringComparison.OrdinalIgnoreCase)
                            || n.Contains("DC Comics", StringComparison.OrdinalIgnoreCase)
                            || n.Contains("DC Entertainment", StringComparison.OrdinalIgnoreCase)
                            || n.Contains("DC Films", StringComparison.OrdinalIgnoreCase)
                            || n.Contains("DC Studios", StringComparison.OrdinalIgnoreCase)))
            return "DC";

        return "Independiente";
    }
}
