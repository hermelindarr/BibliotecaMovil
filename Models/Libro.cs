using SQLite;

namespace BibliotecaMovil.Models;

/// <summary>Entidad Libro. El ORM (sqlite-net) la mapea a la tabla "Libros".</summary>
[Table("Libros")]
public class Libro
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed, MaxLength(150), NotNull]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Autor { get; set; } = string.Empty;

    [Indexed, MaxLength(60)]
    public string Categoria { get; set; } = string.Empty;

    public int Anio { get; set; }

    public bool Disponible { get; set; } = true;

    public DateTime FechaAlta { get; set; } = DateTime.Now;

    // ---- Propiedades solo para la interfaz (no se guardan en la BD) ----

    [Ignore] public bool EsNuevo => (DateTime.Now - FechaAlta).TotalDays <= 30;

    [Ignore] public Color ColorPortada => Categorias.ColorDe(Categoria);

    [Ignore] public string Iniciales => Texto.Iniciales(Titulo);

    [Ignore] public string EstadoTexto => Disponible ? "Disponible" : "Prestado";

    [Ignore] public Color ColorEstado => Disponible ? Paleta.Exito : Paleta.Peligro;

    [Ignore] public string Detalle => $"{Autor} · {Anio}";
}
