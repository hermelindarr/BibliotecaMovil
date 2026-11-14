namespace BibliotecaMovil.Views;

public partial class PrestamosPage : ContentPage
{
    private readonly IPrestamoDAO _prestamos;
    private readonly ILibroDAO _libros;
    private readonly IUsuarioDAO _usuarios;
    private bool _verHistorial;

    public PrestamosPage(IPrestamoDAO prestamos, ILibroDAO libros, IUsuarioDAO usuarios)
    {
        InitializeComponent();
        _prestamos = prestamos;
        _libros = libros;
        _usuarios = usuarios;

        PkrLibro.ItemDisplayBinding = new Binding(nameof(Libro.Titulo));
        PkrUsuario.ItemDisplayBinding = new Binding(nameof(Usuario.Nombre));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Title = Sesion.EsAdmin ? "Préstamos" : "Mis préstamos";
        PanelNuevo.IsVisible = Sesion.EsAdmin;

        try
        {
            if (Sesion.EsAdmin) await CargarOpcionesAsync();
            await CargarListaAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("No se pudieron cargar los préstamos", ex.Message, "Aceptar");
        }
    }

    private async Task CargarOpcionesAsync()
    {
        PkrLibro.ItemsSource = await _libros.DisponiblesAsync();
        PkrUsuario.ItemsSource = await _usuarios.ObtenerTodosAsync();
    }

    private async Task CargarListaAsync()
    {
        int? soloDe = Sesion.EsAdmin ? null : Sesion.Actual?.Id;
        var lista = _verHistorial
            ? await _prestamos.HistorialAsync(soloDe)
            : await _prestamos.ObtenerActivosAsync(soloDe);

        foreach (var prestamo in lista)
            prestamo.PuedeDevolver = Sesion.EsAdmin && prestamo.Activo;

        LblVacio.Text = _verHistorial ? "Aún no hay devoluciones" : "No hay préstamos activos";
        ListaPrestamos.ItemsSource = lista;

        BtnActivos.Style = Paleta.Estilo(_verHistorial ? "BtnChicoContorno" : "BtnChico");
        BtnHistorial.Style = Paleta.Estilo(_verHistorial ? "BtnChico" : "BtnChicoContorno");
    }

    private void OnDiasCambiados(object? sender, ValueChangedEventArgs e)
    {
        var dias = (int)e.NewValue;
        LblDias.Text = dias == 1 ? "1 día" : $"{dias} días";
    }

    private async void OnRegistrar(object? sender, EventArgs e)
    {
        if (PkrLibro.SelectedItem is not Libro libro)
        {
            await DisplayAlertAsync("Falta el libro", "Elige un libro disponible.", "Aceptar");
            return;
        }
        if (PkrUsuario.SelectedItem is not Usuario usuario)
        {
            await DisplayAlertAsync("Falta el usuario", "Elige quién se lleva el libro.", "Aceptar");
            return;
        }

        BtnRegistrar.IsEnabled = false;
        try
        {
            var dias = (int)StpDias.Value;
            await _prestamos.RegistrarAsync(libro.Id, usuario.Id, dias);
            await DisplayAlertAsync(
                "Préstamo registrado",
                $"«{libro.Titulo}» para {usuario.Nombre}. Fecha límite: {DateTime.Today.AddDays(dias):dd/MM/yyyy}.",
                "Aceptar");

            PkrLibro.SelectedItem = null;
            PkrUsuario.SelectedItem = null;
            _verHistorial = false;
            await CargarOpcionesAsync();
            await CargarListaAsync();
        }
        catch (InvalidOperationException ex)
        {
            await DisplayAlertAsync("No se pudo registrar", ex.Message, "Aceptar");
        }
        finally
        {
            BtnRegistrar.IsEnabled = true;
        }
    }

    private async void OnDevolver(object? sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is not PrestamoDetalle prestamo) return;

        var confirmar = await DisplayAlertAsync(
            "Registrar devolución",
            $"¿{prestamo.NombreUsuario} devolvió «{prestamo.TituloLibro}»?",
            "Sí, devolver", "Cancelar");
        if (!confirmar) return;

        try
        {
            await _prestamos.DevolverAsync(prestamo.Id);
            await CargarOpcionesAsync();
            await CargarListaAsync();
        }
        catch (InvalidOperationException ex)
        {
            await DisplayAlertAsync("No se pudo registrar la devolución", ex.Message, "Aceptar");
        }
    }

    private async void OnVerActivos(object? sender, EventArgs e)
    {
        _verHistorial = false;
        await CargarListaAsync();
    }

    private async void OnVerHistorial(object? sender, EventArgs e)
    {
        _verHistorial = true;
        await CargarListaAsync();
    }
}
