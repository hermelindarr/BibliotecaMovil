namespace BibliotecaMovil.Helpers;

/// <summary>Los mismos colores de Resources/Styles/Colors.xaml, para usarlos desde C#.</summary>
public static class Paleta
{
    public static readonly Color Primario = Color.FromArgb("#2563EB");
    public static readonly Color Secundario = Color.FromArgb("#7C3AED");
    public static readonly Color Exito = Color.FromArgb("#16A34A");
    public static readonly Color Peligro = Color.FromArgb("#DC2626");
    public static readonly Color Neutro = Color.FromArgb("#64748B");

    /// <summary>Busca un estilo en los recursos globales (App.xaml).</summary>
    public static Style Estilo(string clave) =>
        Application.Current!.Resources.TryGetValue(clave, out var valor) && valor is Style estilo
            ? estilo
            : throw new KeyNotFoundException($"No existe el estilo '{clave}'.");
}

public static class Texto
{
    private static readonly HashSet<string> Conectores =
        new(StringComparer.OrdinalIgnoreCase) { "de", "del", "la", "las", "el", "los", "en", "y", "a", "un", "una", "the", "of", "in" };

    /// <summary>"Cien años de soledad" → "CA".</summary>
    public static string Iniciales(string? texto)
    {
        var palabras = (texto ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => !Conectores.Contains(p) && char.IsLetterOrDigit(p[0]))
            .Take(2)
            .Select(p => char.ToUpperInvariant(p[0]));
        var resultado = string.Concat(palabras);
        return resultado.Length > 0 ? resultado : "?";
    }

    /// <summary>"Andrew S. Tanenbaum" → "AT" (primer nombre y último apellido).</summary>
    public static string InicialesNombre(string? nombre)
    {
        var palabras = (nombre ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => !Conectores.Contains(p) && char.IsLetter(p[0]))
            .ToList();
        return palabras.Count switch
        {
            0 => "?",
            1 => char.ToUpperInvariant(palabras[0][0]).ToString(),
            _ => $"{char.ToUpperInvariant(palabras[0][0])}{char.ToUpperInvariant(palabras[^1][0])}"
        };
    }
}
