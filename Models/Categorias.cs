namespace BibliotecaMovil.Models;

/// <summary>Catálogo fijo de categorías y su color de portada.</summary>
public static class Categorias
{
    private static readonly Dictionary<string, string> Colores = new()
    {
        ["Programación"] = "#2563EB",
        ["Bases de Datos"] = "#059669",
        ["Redes"] = "#EA580C",
        ["Tecnología"] = "#7C3AED",
        ["Ciencia"] = "#0891B2",
        ["Novela"] = "#DB2777",
        ["Historia"] = "#B45309",
    };

    public static IReadOnlyList<string> Todas { get; } = Colores.Keys.ToList();

    public static Color ColorDe(string? categoria) =>
        categoria is not null && Colores.TryGetValue(categoria, out var hex)
            ? Color.FromArgb(hex)
            : Paleta.Neutro;
}
