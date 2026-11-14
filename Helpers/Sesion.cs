namespace BibliotecaMovil.Helpers;

public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Lector = "Lector";

    public static IReadOnlyList<string> Todos { get; } = new[] { Administrador, Lector };
}

/// <summary>Usuario que inició sesión (control de acceso por rol).</summary>
public static class Sesion
{
    public static Usuario? Actual { get; private set; }

    public static bool EsAdmin => Actual?.EsAdmin == true;

    public static void Iniciar(Usuario usuario) => Actual = usuario;

    public static void Cerrar() => Actual = null;
}
