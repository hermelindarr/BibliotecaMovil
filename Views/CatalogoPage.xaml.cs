namespace BibliotecaMovil.Views;

public partial class CatalogoPage : ContentPage
{
    private readonly ILibroDAO _libros;
    private readonly List<FiltroCategoria> _filtros;
    private FiltroCategoria _filtroActual;
    private CancellationTokenSource? _busqueda;
    private bool _vistaLista;

    public CatalogoPage(ILibroDAO libros)
    {
        InitializeComponent();
        _libros = libros;

        _filtros = new[] { FiltroCategoria.Todos }
            .Concat(Categorias.Todas)
            .Select(nombre => new FiltroCategoria(nombre))
            .ToList();
        _filtroActual = _filtros[0];
        _filtroActual.Seleccionada = true;
        BindableLayout.SetItemsSource(Filtros, _filtros);

        // Diseño adaptable: más columnas en pantallas anchas (tableta / Windows)
        SizeChanged += (_, _) => AjustarColumnas();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        BtnNuevo.IsVisible = Sesion.EsAdmin;
        await CargarAsync();
    }

    private async Task CargarAsync()
    {
        try
        {
            var categoria = _filtroActual.EsTodos ? string.Empty : _filtroActual.Nombre;
            var lista = await _libros.BuscarAsync(Buscador.Text, categoria);
            ListaLibros.ItemsSource = lista;
            LblConteo.Text = lista.Count == 1 ? "1 libro" : $"{lista.Count} libros";
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("No se pudo cargar el catálogo", ex.Message, "Aceptar");
        }
    }

    private async void OnBuscar(object? sender, TextChangedEventArgs e)
    {
        // Espera 300 ms a que el usuario deje de escribir antes de consultar la BD
        _busqueda?.Cancel();
        var token = (_busqueda = new CancellationTokenSource()).Token;
        try
        {
            await Task.Delay(300, token);
            await CargarAsync();
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async void OnFiltroTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not FiltroCategoria filtro || filtro == _filtroActual) return;

        _filtroActual.Seleccionada = false;
        filtro.Seleccionada = true;
        _filtroActual = filtro;
        await CargarAsync();
    }

    private void OnCambiarVista(object? sender, EventArgs e)
    {
        _vistaLista = !_vistaLista;
        BtnVista.Text = _vistaLista ? "▦ Cuadrícula" : "☰ Lista";
        AjustarColumnas();
    }

    private void AjustarColumnas()
    {
        if (ListaLibros.ItemsLayout is not GridItemsLayout cuadricula) return;

        var columnas = _vistaLista ? 1 : Width switch
        {
            > 1000 => 4,
            > 640 => 3,
            _ => 2
        };
        if (cuadricula.Span != columnas) cuadricula.Span = columnas;
    }

    private async void OnNuevo(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(LibroFormPage));

    private async void OnLibroTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not Libro libro) return;

        var detalle = $"Autor: {libro.Autor}\nCategoría: {libro.Categoria}\nAño: {libro.Anio}\nEstado: {libro.EstadoTexto}";

        if (!Sesion.EsAdmin)
        {
            await DisplayAlertAsync(libro.Titulo, detalle, "Cerrar");
            return;
        }

        var accion = await DisplayActionSheetAsync(libro.Titulo, "Cancelar", "Eliminar", "Editar", "Ver detalles");
        switch (accion)
        {
            case "Editar":
                await Shell.Current.GoToAsync($"{nameof(LibroFormPage)}?id={libro.Id}");
                break;
            case "Ver detalles":
                await DisplayAlertAsync(libro.Titulo, detalle, "Cerrar");
                break;
            case "Eliminar":
                await EliminarAsync(libro);
                break;
        }
    }

    private async Task EliminarAsync(Libro libro)
    {
        var confirmar = await DisplayAlertAsync(
            "Eliminar libro",
            $"«{libro.Titulo}» se borrará del catálogo junto con su historial de préstamos. Esta acción no se puede deshacer.",
            "Eliminar", "Cancelar");
        if (!confirmar) return;

        try
        {
            await _libros.EliminarAsync(libro.Id);
            await CargarAsync();
        }
        catch (InvalidOperationException ex)
        {
            await DisplayAlertAsync("No se puede eliminar", ex.Message, "Aceptar");
        }
    }
}
