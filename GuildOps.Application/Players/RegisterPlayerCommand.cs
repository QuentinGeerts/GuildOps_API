using System.ComponentModel.DataAnnotations;

namespace GuildOps.Application.Players;

public sealed record RegisterPlayerCommand(
    [Required, StringLength(256, MinimumLength = 3)] string AccountName,
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(128, MinimumLength = 8)] string Password);
