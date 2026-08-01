using TrackMuvi.Shared.Enums;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Data.Repositories;

public interface IPersonalListRepository
{
    Task<PersonalStatusDto> GetStatusAsync(string titleKey, CancellationToken ct = default);

    /// <summary>Todos los estados guardados (para pintar Mi Lista / "Series que sigues" en Inicio).</summary>
    Task<IReadOnlyList<PersonalStatusDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Prende/apaga un flag (Seguir, Favorita, etc.) y devuelve el estado resultante.</summary>
    Task<PersonalStatusDto> ToggleFlagAsync(string titleKey, PersonalStatusFlag flag, CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetKeysByFlagAsync(PersonalStatusFlag flag, CancellationToken ct = default);

    Task AddViewHistoryEntryAsync(string titleKey, DateTimeOffset watchedAt, CancellationToken ct = default);

    Task<IReadOnlyList<ViewHistoryEntryDto>> GetViewHistoryAsync(string titleKey, CancellationToken ct = default);
}
