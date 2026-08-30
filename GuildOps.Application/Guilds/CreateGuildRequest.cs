using System.ComponentModel.DataAnnotations;

namespace GuildOps.Application.Guilds;

public sealed record CreateGuildRequest(
    [Required] Guid CharacterId,
    [Required, StringLength(256, MinimumLength = 2)] string Name,
    [StringLength(2000)] string? Description = null,
    [StringLength(512), Url] string? ChatUrl = null);
