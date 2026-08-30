using GuildOps.Domain.Games;

namespace GuildOps.Application.Games;

public sealed record GameRoleDto(Guid Id, string Name)
{
    public static GameRoleDto From(GameRole role) => new(role.Id, role.Name);
}
