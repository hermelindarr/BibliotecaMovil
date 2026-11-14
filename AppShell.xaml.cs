namespace BibliotecaMovil;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        // Pantalla de registro/edición: se abre encima de la pestaña actual
        Routing.RegisterRoute(nameof(LibroFormPage), typeof(LibroFormPage));
    }
}
