using GuildOps.Application.Abstractions;
using GuildOps.Domain.Guilds;

namespace GuildOps.Application.Guilds;

internal sealed class CreateGuildCommandHandler(
    IPlayerRepository players,
    IGuildRepository guilds,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateGuildCommand, CreateGuildResult>
{
    private const string ServerNameIndex = "IX_Guilds_Server_Name";
    private const string MembershipIndex = "IX_GuildMemberships_CharacterId";

    public async Task<CreateGuildResult> HandleAsync(CreateGuildCommand command, CancellationToken cancellationToken = default)
    {
        var character = await players.GetCharacterAsync(command.CharacterId, cancellationToken);

        if (character is null || character.PlayerId != command.PlayerId)
        {
            return CreateGuildResult.Rejected(CreateGuildOutcome.CharacterNotFound);
        }

        if (await guilds.CharacterHasMembershipAsync(character.Id, cancellationToken))
        {
            return CreateGuildResult.Rejected(CreateGuildOutcome.CharacterAlreadyInGuild);
        }

        if (await guilds.NameExistsOnServerAsync(character.Server, command.Name, cancellationToken))
        {
            return CreateGuildResult.Rejected(CreateGuildOutcome.NameTakenOnServer);
        }

        var guild = new Guild(character.GameId, command.Name, character.Server, command.Description, command.ChatUrl);

        var leader = DefaultGuildRanks.Leader(guild.Id);
        guild.Ranks.Add(leader);
        guild.Ranks.Add(DefaultGuildRanks.Officer(guild.Id));
        guild.Ranks.Add(DefaultGuildRanks.Member(guild.Id));

        guild.Memberships.Add(new GuildMembership(guild.Id, character.Id, leader.Id));

        guilds.Add(guild);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException exception) when (exception.ConstraintName == ServerNameIndex)
        {
            return CreateGuildResult.Rejected(CreateGuildOutcome.NameTakenOnServer);
        }
        catch (UniqueConstraintException exception) when (exception.ConstraintName == MembershipIndex)
        {
            return CreateGuildResult.Rejected(CreateGuildOutcome.CharacterAlreadyInGuild);
        }

        return CreateGuildResult.Created(GuildDto.From(guild));
    }
}
