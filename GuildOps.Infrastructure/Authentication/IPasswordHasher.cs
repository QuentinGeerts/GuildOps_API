namespace GuildOps.Infrastructure.Authentication;

internal interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string? encodedHash);
}
