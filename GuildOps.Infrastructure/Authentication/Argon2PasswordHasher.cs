using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace GuildOps.Infrastructure.Authentication;

internal sealed class Argon2PasswordHasher : IPasswordHasher
{
    private const int MemoryKiB = 19456;
    private const int Iterations = 2;
    private const int Parallelism = 1;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    private static readonly byte[] DummySalt = new byte[SaltSize];

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Compute(password, salt, MemoryKiB, Iterations, Parallelism);

        return string.Create(CultureInfo.InvariantCulture,
            $"$argon2id$v=19$m={MemoryKiB},t={Iterations},p={Parallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}");
    }

    public bool Verify(string password, string? encodedHash)
    {
        if (encodedHash is null)
        {
            Compute(password, DummySalt, MemoryKiB, Iterations, Parallelism);
            return false;
        }

        string[] parts = encodedHash.Split('$');
        if (parts.Length != 6 || parts[1] != "argon2id")
        {
            return false;
        }

        if (!TryReadParameters(parts[3], out int memoryKiB, out int iterations, out int parallelism))
        {
            return false;
        }

        byte[] salt = Convert.FromBase64String(parts[4]);
        byte[] expected = Convert.FromBase64String(parts[5]);
        byte[] actual = Compute(password, salt, memoryKiB, iterations, parallelism);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static byte[] Compute(string password, byte[] salt, int memoryKiB, int iterations, int parallelism)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = memoryKiB,
            Iterations = iterations,
            DegreeOfParallelism = parallelism
        };

        return argon2.GetBytes(HashSize);
    }

    private static bool TryReadParameters(string segment, out int memoryKiB, out int iterations, out int parallelism)
    {
        memoryKiB = iterations = parallelism = 0;

        foreach (string pair in segment.Split(','))
        {
            string[] keyValue = pair.Split('=');
            if (keyValue.Length != 2 || !int.TryParse(keyValue[1], CultureInfo.InvariantCulture, out int value))
            {
                return false;
            }

            switch (keyValue[0])
            {
                case "m": memoryKiB = value; break;
                case "t": iterations = value; break;
                case "p": parallelism = value; break;
                default: return false;
            }
        }

        return memoryKiB > 0 && iterations > 0 && parallelism > 0;
    }
}
