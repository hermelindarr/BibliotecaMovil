namespace BibliotecaMovil.Views;

/// <summary>Se abre con "LibroFormPage" (nuevo) o "LibroFormPage?id=5" (editar).</summary>
[QueryProperty(nameof(LibroId), "id")]
public partial class LibroFormPage : ContentPage
{
    private readonly ILibroDAO _libros;
    private Libro _libro = new();
    private bool _cargado;

    public string? LibroId { get; set; }

    public LibroFormPage(ILibroDAO libros)
    {
        InitializeComponent();
        _libros = libros;
        PkrCategoria.ItemsSource = Categorias.Todas.ToList();
        EntAnio.Text = DateTime.Now.Year.ToString();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_cargado) return;
        _cargado = true;

        if (!Sesion.EsAdmin)
        {
            await DisplayAlertAsync("Acceso restringido", "Solo un administrador puede registrar o editar libros.", "Aceptar");
            await Shell.Current.GoToAsync("..");
            return;
        }

        if (int.TryParse(LibroId, out var id) && id > 0)
        {
            var libro = await _libros.ObtenerPorIdAsync(id);
            if (libro is null)
            {
                await DisplayAlertAsync("Libro no encontrado", "Es posible que otra persona lo haya eliminado.", "Aceptar");
                await Shell.Current.GoToAsync("..");
                return;
            }

            _libro = libro;
            Title = "Editar libro";
            BtnGuardar.Text = "Guardar cambios";
            BtnEliminar.IsVisible = true;
            EntTitulo.Text = libro.Titulo;
            EntAutor.Text = libro.Autor;
            PkrCategoria.SelectedItem = libro.Categoria;
            EntAnio.Text = libro.Anio.ToString();
        }

        ActualizarPortada();
    }

    private void OnTituloCambiado(object? sender, TextChangedEventArgs e) => ActualizarPortada();

    private void OnCategoriaCambiada(object? sender, EventArgs e) => ActualizarPortada();

    private void ActualizarPortada()
    {
        Portada.BackgroundColor = Categorias.ColorDe(PkrCategoria.SelectedItem as string);
        LblIniciales.Text = Texto.Iniciales(EntTitulo.Text);
        LblEstado.Text = _libro.EstadoTexto;
        InsigniaEstado.BackgroundColor = _libro.ColorEstado;
    }

    private async void OnGuardar(object? sender, EventArgs e)
    {
        var titulo = EntTitulo.Text?.Trim() ?? string.Empty;
        var autor = EntAutor.Text?.Trim() ?? string.Empty;
        var categoria = PkrCategoria.SelectedItem as string;
        var anioValido = int.TryParse(EntAnio.Text, out var anio) && anio >= 1450 && anio <= DateTime.Now.Year;

        var error =
            titulo.Length == 0 ? "Escribe el título del libro." :
            autor.Length == 0 ? "Escribe el nombre del autor." :
            categoria is null ? "Elige una categoría." :
            !anioValido ? $"Escribe un año entre 1450 y {DateTime.Now.Year}." :
            null;

        LblError.Text = error;
        LblError.IsVisible = error is not null;
        if (error is not null) return;

        _libro.Titulo = titulo;
        _libro.Autor = autor;
        _libro.Categoria = categoria!;
        _libro.Anio = anio;

        BtnGuardar.IsEnabled = false;
        try
        {
            await _libros.GuardarAsync(_libro);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            LblError.Text = $"No se pudo guardar: {ex.Message}";
            LblError.IsVisible = true;
        }
        finally
        {
            BtnGuardar.IsEnabled = true;
        }
    }

    private async void OnEliminar(object? sender, EventArgs e)
    {
        var confirmar = await DisplayAlertAsync(
            "Eliminar libro",
            $"«{_libro.Titulo}» se borrará del catálogo junto con su historial de préstamos. Esta acción no se puede deshacer.",
            "Eliminar", "Cancelar");
        if (!confirmar) return;

        try
        {
            await _libros.EliminarAsync(_libro.Id);
            await Shell.Current.GoToAsync("..");
        }
        catch (InvalidOperationException ex)
        {
            await DisplayAlertAsync("No se puede eliminar", ex.Message, "Aceptar");
        }
    }

    private async void OnCancelar(object? sender, EventArgs e) => await Shell.Current.GoToAsync("..");
}
