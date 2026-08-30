using System.ComponentModel.DataAnnotations;

namespace GuildOps.Application.Guilds;

public sealed record ApplyToGuildRequest(
    [Required] Guid CharacterId,
    [StringLength(1000)] string? Message = null);
