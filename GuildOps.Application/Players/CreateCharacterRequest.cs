using System.ComponentModel.DataAnnotations;

namespace GuildOps.Application.Players;

public sealed record CreateCharacterRequest(
    [Required] Guid GameId,
    [Required] Guid CharacterClassId,
    [Required, StringLength(256, MinimumLength = 2)] string Name,
    [Required, StringLength(256, MinimumLength = 2)] string Server,
    [Range(1, int.MaxValue)] int Level = 1);
