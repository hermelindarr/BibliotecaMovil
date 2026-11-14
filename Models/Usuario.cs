using SQLite;

namespace BibliotecaMovil.Models;

[Table("Usuarios")]
public class Usuario
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(120), NotNull]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Nombre para iniciar sesión (siempre en minúsculas).</summary>
    [Unique, MaxLength(30), NotNull]
    public string NombreUsuario { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Correo { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Rol { get; set; } = Roles.Lector;

    /// <summary>Nunca se guarda la contraseña: solo su hash PBKDF2 y la sal.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string Salt { get; set; } = string.Empty;

    [Ignore] public bool EsAdmin => Rol == Roles.Administrador;

    [Ignore] public string Iniciales => Texto.InicialesNombre(Nombre);

    [Ignore] public Color ColorAvatar => EsAdmin ? Paleta.Secundario : Paleta.Primario;

    [Ignore] public string Descripcion => $"@{NombreUsuario} · {Rol}";
}
