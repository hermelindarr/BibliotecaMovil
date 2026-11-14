namespace BibliotecaMovil.Models;

/// <summary>Chip de filtro del catálogo (FlexLayout + BindableLayout).</summary>
public class FiltroCategoria(string nombre) : ObservableObject
{
    public const string Todos = "Todos";

    private bool _seleccionada;

    public string Nombre { get; } = nombre;

    public bool EsTodos => Nombre == Todos;

    private Color ColorBase => EsTodos ? Paleta.Primario : Categorias.ColorDe(Nombre);

    public bool Seleccionada
    {
        get => _seleccionada;
        set
        {
            if (_seleccionada == value) return;
            _seleccionada = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Fondo));
            OnPropertyChanged(nameof(ColorTexto));
        }
    }

    public Color Fondo => Seleccionada ? ColorBase : ColorBase.WithAlpha(0.14f);

    public Color ColorTexto => Seleccionada ? Colors.White : ColorBase;
}
