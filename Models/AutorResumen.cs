using SQLite;

namespace BibliotecaMovil.Models;

/// <summary>Resultado de "SELECT Autor, COUNT(*) AS Libros ... GROUP BY Autor".</summary>
public class AutorResumen
{
    public string Autor { get; set; } = string.Empty;

    public int Libros { get; set; }

    [Ignore] public string Iniciales => Texto.Iniciales(Autor);

    [Ignore] public string LibrosTexto => Libros == 1 ? "1 libro" : $"{Libros} libros";

    [Ignore] public Color Color
    {
        get
        {
            var colores = Categorias.Todas;
            var indice = Math.Abs(Autor.Sum(c => c)) % colores.Count;
            return Categorias.ColorDe(colores[indice]);
        }
    }
}
