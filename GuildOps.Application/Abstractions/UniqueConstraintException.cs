namespace GuildOps.Application.Abstractions;

public sealed class UniqueConstraintException(string constraintName, Exception innerException)
    : Exception($"Violation de la contrainte d'unicite '{constraintName}'.", innerException)
{
    public string ConstraintName { get; } = constraintName;
}
