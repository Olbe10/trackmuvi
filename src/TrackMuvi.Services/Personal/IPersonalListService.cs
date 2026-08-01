using TrackMuvi.Shared.Enums;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Services.Personal;

/// <summary>Orquesta el estado personal (SQLite, vía TrackMuvi.Data) para las pantallas Mi Lista/Detail/Inicio.</summary>
public interface IPersonalListService
{
    Task<PersonalStatusDto> GetStatusAsync(string titleKey, CancellationToken ct = default);

    /// <summary>Prende/apaga un flag (Seguir, Favorita, Quiero ver...). Si el flag es Watched y queda en
    /// true, además registra una entrada de historial con la fecha/hora actual.</summary>
    Task<PersonalStatusDto> ToggleFlagAsync(string titleKey, PersonalStatusFlag flag, CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetKeysByFlagAsync(PersonalStatusFlag flag, CancellationToken ct = default);

    Task<IReadOnlyList<ViewHistoryEntryDto>> GetHistoryAsync(string titleKey, CancellationToken ct = default);
}
