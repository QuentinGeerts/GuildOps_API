using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GuildOps.Infrastructure.Persistence;

internal static class UniqueConstraintReader
{
    private const int DuplicateKeyRow = 2601;
    private const int DuplicateKeyConstraint = 2627;

    public static bool TryRead(DbUpdateException exception, [NotNullWhen(true)] out string? constraintName)
    {
        constraintName = null;

        if (exception.InnerException is not SqlException sql)
        {
            return false;
        }

        if (sql.Number is not (DuplicateKeyRow or DuplicateKeyConstraint))
        {
            return false;
        }

        // Le message cite l'objet ("dbo.Players") puis l'index ("IX_Players_AccountName").
        // On retient le premier nom cite qui n'est pas qualifie par un schema : c'est l'index.
        string[] segments = sql.Message.Split('\'');

        for (int i = 1; i < segments.Length; i += 2)
        {
            if (!segments[i].Contains('.'))
            {
                constraintName = segments[i];
                return true;
            }
        }

        return false;
    }
}
