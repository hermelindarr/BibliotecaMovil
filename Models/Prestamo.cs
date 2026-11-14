using SQLite;

namespace BibliotecaMovil.Models;

[Table("Prestamos")]
public class Prestamo
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed] public int IdLibro { get; set; }

    [Indexed] public int IdUsuario { get; set; }

    public DateTime FechaPrestamo { get; set; } = DateTime.Now;

    public DateTime FechaLimite { get; set; }

    /// <summary>Null mientras el libro no se ha devuelto.</summary>
    public DateTime? FechaDevolucion { get; set; }
}

/// <summary>
/// Resultado de la consulta con JOIN (Prestamos + Libros + Usuarios).
/// No es una tabla: solo recibe las columnas que la pantalla necesita.
/// </summary>
public class PrestamoDetalle
{
    public int Id { get; set; }
    public int IdLibro { get; set; }
    public int IdUsuario { get; set; }
    public string TituloLibro { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaLimite { get; set; }
    public DateTime? FechaDevolucion { get; set; }

    /// <summary>Lo decide la pantalla según el rol del usuario.</summary>
    [Ignore] public bool PuedeDevolver { get; set; }

    [Ignore] public bool Activo => FechaDevolucion is null;

    [Ignore] public bool Vencido => Activo && FechaLimite.Date < DateTime.Today;

    [Ignore] public Color ColorPortada => Categorias.ColorDe(Categoria);

    [Ignore] public string Iniciales => Texto.Iniciales(TituloLibro);

    [Ignore] public string Fechas => $"Prestado {FechaPrestamo:dd/MM/yyyy} · Límite {FechaLimite:dd/MM/yyyy}";

    [Ignore] public string EstadoTexto
    {
        get
        {
            if (!Activo) return $"Devuelto {FechaDevolucion:dd/MM}";
            var dias = (FechaLimite.Date - DateTime.Today).Days;
            return dias switch
            {
                < 0 => $"Vencido ({-dias} d)",
                0 => "Vence hoy",
                1 => "Vence mañana",
                _ => $"Vence en {dias} días"
            };
        }
    }

    [Ignore] public Color ColorEstado => !Activo ? Paleta.Neutro : Vencido ? Paleta.Peligro : Paleta.Exito;
}
