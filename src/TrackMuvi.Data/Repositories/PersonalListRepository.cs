using Microsoft.EntityFrameworkCore;
using TrackMuvi.Data.Entities;
using TrackMuvi.Shared.Enums;
using TrackMuvi.Shared.Models;

namespace TrackMuvi.Data.Repositories;

public class PersonalListRepository(TrackMuviDbContext db) : IPersonalListRepository
{
    public async Task<PersonalStatusDto> GetStatusAsync(string titleKey, CancellationToken ct = default)
    {
        var entity = await db.PersonalStatuses.FindAsync([titleKey], ct);
        return entity is null ? PersonalStatusDto.Empty(titleKey) : ToDto(entity);
    }

    public async Task<IReadOnlyList<PersonalStatusDto>> GetAllAsync(CancellationToken ct = default) =>
        await db.PersonalStatuses.Select(e => ToDto(e)).ToListAsync(ct);

    public async Task<PersonalStatusDto> ToggleFlagAsync(string titleKey, PersonalStatusFlag flag, CancellationToken ct = default)
    {
        var entity = await db.PersonalStatuses.FindAsync([titleKey], ct);
        if (entity is null)
        {
            entity = new PersonalStatusEntity { TitleKey = titleKey };
            db.PersonalStatuses.Add(entity);
        }

        SetFlag(entity, flag, !GetFlag(entity, flag));
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<IReadOnlyList<string>> GetKeysByFlagAsync(PersonalStatusFlag flag, CancellationToken ct = default)
    {
        var query = flag switch
        {
            PersonalStatusFlag.Want => db.PersonalStatuses.Where(e => e.Want),
            PersonalStatusFlag.Watched => db.PersonalStatuses.Where(e => e.Watched),
            PersonalStatusFlag.Favorite => db.PersonalStatuses.Where(e => e.Favorite),
            PersonalStatusFlag.Following => db.PersonalStatuses.Where(e => e.Following),
            PersonalStatusFlag.Pending => db.PersonalStatuses.Where(e => e.Pending),
            PersonalStatusFlag.Abandoned => db.PersonalStatuses.Where(e => e.Abandoned),
            _ => throw new ArgumentOutOfRangeException(nameof(flag))
        };

        return await query.Select(e => e.TitleKey).ToListAsync(ct);
    }

    public async Task AddViewHistoryEntryAsync(string titleKey, DateTimeOffset watchedAt, CancellationToken ct = default)
    {
        db.ViewHistory.Add(new ViewHistoryEntity { TitleKey = titleKey, WatchedAt = watchedAt });
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ViewHistoryEntryDto>> GetViewHistoryAsync(string titleKey, CancellationToken ct = default)
    {
        // El provider de SQLite para EF Core no traduce ORDER BY sobre columnas DateTimeOffset
        // (ni siquiera vía .Ticks); se trae la lista (siempre pequeña, por título) y se ordena en memoria.
        var entries = await db.ViewHistory.Where(e => e.TitleKey == titleKey).ToListAsync(ct);
        return entries
            .OrderByDescending(e => e.WatchedAt)
            .Select(e => new ViewHistoryEntryDto(e.TitleKey, e.WatchedAt))
            .ToList();
    }

    private static bool GetFlag(PersonalStatusEntity e, PersonalStatusFlag flag) => flag switch
    {
        PersonalStatusFlag.Want => e.Want,
        PersonalStatusFlag.Watched => e.Watched,
        PersonalStatusFlag.Favorite => e.Favorite,
        PersonalStatusFlag.Following => e.Following,
        PersonalStatusFlag.Pending => e.Pending,
        PersonalStatusFlag.Abandoned => e.Abandoned,
        _ => throw new ArgumentOutOfRangeException(nameof(flag))
    };

    private static void SetFlag(PersonalStatusEntity e, PersonalStatusFlag flag, bool value)
    {
        switch (flag)
        {
            case PersonalStatusFlag.Want: e.Want = value; break;
            case PersonalStatusFlag.Watched: e.Watched = value; break;
            case PersonalStatusFlag.Favorite: e.Favorite = value; break;
            case PersonalStatusFlag.Following: e.Following = value; break;
            case PersonalStatusFlag.Pending: e.Pending = value; break;
            case PersonalStatusFlag.Abandoned: e.Abandoned = value; break;
            default: throw new ArgumentOutOfRangeException(nameof(flag));
        }
    }

    private static PersonalStatusDto ToDto(PersonalStatusEntity e) => new(
        e.TitleKey, e.Want, e.Watched, e.Favorite, e.Following, e.Pending, e.Abandoned, e.UpdatedAt);
}
