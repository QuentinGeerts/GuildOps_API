using GuildOps.Application.Abstractions;
using GuildOps.Domain.Players;

namespace GuildOps.Application.Players;

internal sealed class RegisterPlayerCommandHandler(
    IPlayerRepository players,
    IPlayerCredentialStore credentials,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterPlayerCommand, RegisterPlayerResult>
{
    private const string AccountNameIndex = "IX_Players_AccountName";
    private const string EmailIndex = "IX_PlayerCredentials_Email";

    public async Task<RegisterPlayerResult> HandleAsync(RegisterPlayerCommand command, CancellationToken cancellationToken = default)
    {
        if (await players.AccountNameExistsAsync(command.AccountName, cancellationToken))
        {
            return RegisterPlayerResult.Rejected(RegisterPlayerOutcome.AccountNameTaken);
        }

        if (await credentials.EmailExistsAsync(command.Email, cancellationToken))
        {
            return RegisterPlayerResult.Rejected(RegisterPlayerOutcome.EmailTaken);
        }

        var player = new Player(command.AccountName);

        players.Add(player);
        credentials.Create(player.Id, command.Email, command.Password);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException exception) when (exception.ConstraintName == AccountNameIndex)
        {
            return RegisterPlayerResult.Rejected(RegisterPlayerOutcome.AccountNameTaken);
        }
        catch (UniqueConstraintException exception) when (exception.ConstraintName == EmailIndex)
        {
            return RegisterPlayerResult.Rejected(RegisterPlayerOutcome.EmailTaken);
        }

        return RegisterPlayerResult.Created(RegisteredPlayerDto.From(player));
    }
}
