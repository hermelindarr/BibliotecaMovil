using System.Security.Cryptography;

namespace BibliotecaMovil.Helpers;

/// <summary>Hash de contraseñas con PBKDF2-SHA256 y sal aleatoria.</summary>
public static class PasswordHasher
{
    private const int Iteraciones = 50_000;
    private const int Longitud = 32;

    public static (string Hash, string Salt) Crear(string password)
    {
        var sal = RandomNumberGenerator.GetBytes(16);
        return (Convert.ToBase64String(Derivar(password, sal)), Convert.ToBase64String(sal));
    }

    public static bool Verificar(string password, string hash, string salt)
    {
        if (string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt)) return false;
        var esperado = Convert.FromBase64String(hash);
        var calculado = Derivar(password, Convert.FromBase64String(salt));
        return CryptographicOperations.FixedTimeEquals(esperado, calculado);
    }

    private static byte[] Derivar(string password, byte[] sal) =>
        Rfc2898DeriveBytes.Pbkdf2(password, sal, Iteraciones, HashAlgorithmName.SHA256, Longitud);
}
