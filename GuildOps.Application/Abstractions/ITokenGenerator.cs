namespace GuildOps.Application.Abstractions;

public interface ITokenGenerator
{
    AccessToken Generate(Guid playerId);
}
