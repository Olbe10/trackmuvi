using TrackMuvi.Data.Entities;

namespace TrackMuvi.Data.Repositories;

/// <summary>Snapshot local de título/poster/fecha de estreno para los títulos que el usuario sigue,
/// usado para poder agendar/evaluar notificaciones sin depender de que la API esté disponible
/// en ese momento.</summary>
public interface ITitleCacheRepository
{
    Task<TitleCacheEntity?> GetAsync(string titleKey, CancellationToken ct = default);

    Task<IReadOnlyList<TitleCacheEntity>> GetAllAsync(CancellationToken ct = default);

    Task UpsertAsync(TitleCacheEntity entity, CancellationToken ct = default);

    Task RemoveAsync(string titleKey, CancellationToken ct = default);
}
