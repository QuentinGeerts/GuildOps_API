using System.ComponentModel.DataAnnotations;

namespace GuildOps.Application.Guilds;

public sealed record InviteCharacterRequest(
    [Required] Guid CharacterId,
    [StringLength(1000)] string? Message = null);
