using Microsoft.Maui.Layouts;

namespace BibliotecaMovil.Controls;

/// <summary>
/// Actividad 7 · Custom Layout: "Mosaico de accesos".
/// Acomoda a sus hijos en un patrón que se repite: una celda destacada a lo ancho
/// y después <see cref="Columnas"/> celdas normales. Los hijos ocultos no ocupan lugar.
///
///   ┌───────────────┐
///   │   destacada   │
///   ├───────┬───────┤
///   │ celda │ celda │
///   ├───────┴───────┤
///   │   destacada   │
///   └───────────────┘
/// </summary>
public class MosaicoLayout : Layout
{
    public static readonly BindableProperty ColumnasProperty =
        BindableProperty.Create(nameof(Columnas), typeof(int), typeof(MosaicoLayout), 2, propertyChanged: Invalidar);

    public static readonly BindableProperty EspacioProperty =
        BindableProperty.Create(nameof(Espacio), typeof(double), typeof(MosaicoLayout), 12d, propertyChanged: Invalidar);

    public static readonly BindableProperty AltoCeldaProperty =
        BindableProperty.Create(nameof(AltoCelda), typeof(double), typeof(MosaicoLayout), 110d, propertyChanged: Invalidar);

    public int Columnas
    {
        get => (int)GetValue(ColumnasProperty);
        set => SetValue(ColumnasProperty, value);
    }

    public double Espacio
    {
        get => (double)GetValue(EspacioProperty);
        set => SetValue(EspacioProperty, value);
    }

    public double AltoCelda
    {
        get => (double)GetValue(AltoCeldaProperty);
        set => SetValue(AltoCeldaProperty, value);
    }

    private static void Invalidar(BindableObject bindable, object anterior, object nuevo) =>
        ((IView)bindable).InvalidateMeasure();

    protected override ILayoutManager CreateLayoutManager() => new MosaicoLayoutManager(this);
}

/// <summary>Reglas de medición (Measure) y acomodo (ArrangeChildren) del mosaico.</summary>
public class MosaicoLayoutManager(MosaicoLayout layout) : ILayoutManager
{
    public Size Measure(double widthConstraint, double heightConstraint)
    {
        var padding = layout.Padding;
        var ancho = double.IsInfinity(widthConstraint) ? 360 : widthConstraint;
        var celdas = Calcular(ancho - padding.HorizontalThickness);

        double alto = 0;
        foreach (var (vista, rect) in celdas)
        {
            vista.Measure(rect.Width, rect.Height);
            alto = Math.Max(alto, rect.Bottom);
        }

        return new Size(ancho, alto + padding.VerticalThickness);
    }

    public Size ArrangeChildren(Rect bounds)
    {
        var padding = layout.Padding;
        foreach (var (vista, rect) in Calcular(bounds.Width - padding.HorizontalThickness))
        {
            vista.Arrange(new Rect(
                bounds.X + padding.Left + rect.X,
                bounds.Y + padding.Top + rect.Y,
                rect.Width,
                rect.Height));
        }

        return bounds.Size;
    }

    private List<(IView Vista, Rect Rect)> Calcular(double ancho)
    {
        ancho = Math.Max(0, ancho);
        var columnas = Math.Max(1, layout.Columnas);
        var espacio = layout.Espacio;
        var alto = layout.AltoCelda;
        var anchoCelda = Math.Max(0, (ancho - espacio * (columnas - 1)) / columnas);

        var resultado = new List<(IView, Rect)>();
        int visibles = 0, columna = 0;
        double y = 0;

        foreach (var vista in layout)
        {
            if (vista.Visibility == Visibility.Collapsed) continue;

            var destacada = visibles % (columnas + 1) == 0;
            if (destacada)
            {
                if (columna != 0) { y += alto + espacio; columna = 0; }
                resultado.Add((vista, new Rect(0, y, ancho, alto)));
                y += alto + espacio;
            }
            else
            {
                resultado.Add((vista, new Rect(columna * (anchoCelda + espacio), y, anchoCelda, alto)));
                if (++columna == columnas) { columna = 0; y += alto + espacio; }
            }

            visibles++;
        }

        return resultado;
    }
}
