using System.ComponentModel.DataAnnotations;

namespace GuildOps.Application.Players;

public sealed record LogoutCommand([Required, StringLength(256)] string RefreshToken);
