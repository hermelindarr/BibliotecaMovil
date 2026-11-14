namespace BibliotecaMovil.Views;

public partial class InicioPage : ContentPage
{
    private readonly ILibroDAO _libros;
    private readonly IPrestamoDAO _prestamos;

    public InicioPage(ILibroDAO libros, IPrestamoDAO prestamos)
    {
        InitializeComponent();
        _libros = libros;
        _prestamos = prestamos;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var usuario = Sesion.Actual;
        LblSaludo.Text = $"Hola, {usuario?.Nombre ?? "lector"} 👋";
        LblRol.Text = usuario is null ? string.Empty : $"{usuario.Rol} · @{usuario.NombreUsuario}";

        // Control de acceso: un lector no ve las opciones de administración
        TileNuevo.IsVisible = Sesion.EsAdmin;
        TileUsuarios.IsVisible = Sesion.EsAdmin;
        LblTilePrestamos.Text = Sesion.EsAdmin ? "Préstamos" : "Mis préstamos";
        LblPrestadosTexto.Text = Sesion.EsAdmin ? "Prestados" : "Míos";

        try
        {
            var (total, disponibles) = await _libros.ContarAsync();
            var prestados = await _prestamos.ContarActivosAsync(Sesion.EsAdmin ? null : usuario?.Id);

            LblTotal.Text = total.ToString();
            LblDisponibles.Text = disponibles.ToString();
            LblPrestados.Text = prestados.ToString();

            BindableLayout.SetItemsSource(ListaAutores, await _libros.AutoresDestacadosAsync(8));
            BindableLayout.SetItemsSource(ListaRecientes, await _libros.RecientesAsync(3));
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("No se pudieron cargar los datos", ex.Message, "Aceptar");
        }
    }

    private async void OnIrCatalogo(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("//catalogo");

    private async void OnIrPrestamos(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("//prestamos");

    private async void OnIrUsuarios(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("//usuarios");

    private async void OnNuevoLibro(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync(nameof(LibroFormPage));

    private async void OnReporte(object? sender, TappedEventArgs e)
    {
        try
        {
            var (total, disponibles) = await _libros.ContarAsync();
            var activos = await _prestamos.ContarActivosAsync();
            var vencidos = await _prestamos.ContarVencidosAsync();
            var porcentaje = total == 0 ? 0 : disponibles * 100 / total;

            await DisplayAlertAsync(
                "Reporte de la biblioteca",
                $"Libros en el catálogo: {total}\n" +
                $"Disponibles: {disponibles} ({porcentaje} %)\n" +
                $"Préstamos activos: {activos}\n" +
                $"Préstamos vencidos: {vencidos}",
                "Cerrar");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("No se pudo generar el reporte", ex.Message, "Aceptar");
        }
    }

    private async void OnCerrarSesion(object? sender, TappedEventArgs e)
    {
        if (await DisplayAlertAsync("Cerrar sesión", "¿Quieres salir de tu cuenta?", "Salir", "Cancelar"))
            ((App)Application.Current!).CerrarSesion();
    }
}
