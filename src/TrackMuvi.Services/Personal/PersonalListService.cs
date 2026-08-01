using TrackMuvi.Data.Repositories;
using TrackMuvi.Shared.Enums;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Services.Personal;

public class PersonalListService(IPersonalListRepository repository) : IPersonalListService
{
    public Task<PersonalStatusDto> GetStatusAsync(string titleKey, CancellationToken ct = default) =>
        repository.GetStatusAsync(titleKey, ct);

    public async Task<PersonalStatusDto> ToggleFlagAsync(string titleKey, PersonalStatusFlag flag, CancellationToken ct = default)
    {
        var result = await repository.ToggleFlagAsync(titleKey, flag, ct);
        if (flag == PersonalStatusFlag.Watched && result.Watched)
        {
            await repository.AddViewHistoryEntryAsync(titleKey, DateTimeOffset.UtcNow, ct);
        }

        return result;
    }

    public Task<IReadOnlyList<string>> GetKeysByFlagAsync(PersonalStatusFlag flag, CancellationToken ct = default) =>
        repository.GetKeysByFlagAsync(flag, ct);

    public Task<IReadOnlyList<ViewHistoryEntryDto>> GetHistoryAsync(string titleKey, CancellationToken ct = default) =>
        repository.GetViewHistoryAsync(titleKey, ct);
}
