using System.ComponentModel.DataAnnotations;

namespace GuildOps.Application.Guilds;

public sealed record EditGuildProfileRequest(
    [Required, StringLength(256, MinimumLength = 2)] string Name,
    [StringLength(2000)] string? Description = null,
    [StringLength(512), Url] string? ChatUrl = null);

public sealed record EditGuildProfileCommand(Guid PlayerId, Guid GuildId, string Name, string? Description, string? ChatUrl)
{
    public static EditGuildProfileCommand From(Guid playerId, Guid guildId, EditGuildProfileRequest request)
        => new(playerId, guildId, request.Name, request.Description, request.ChatUrl);
}

public enum EditGuildProfileOutcome
{
    Updated = 1,
    Forbidden = 2,
    GuildNotFound = 3,
    NameTakenOnServer = 4
}
