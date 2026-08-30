using System.ComponentModel.DataAnnotations;

namespace GuildOps.Application.Players;

public sealed record LoginCommand(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(128)] string Password);
