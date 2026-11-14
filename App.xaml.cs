using Microsoft.Extensions.DependencyInjection;

namespace BibliotecaMovil;

public partial class App : Application
{
    private readonly IServiceProvider _servicios;

    public App(IServiceProvider servicios)
    {
        InitializeComponent();
        _servicios = servicios;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var ventana = new Window(_servicios.GetRequiredService<LoginPage>()) { Title = "Biblioteca Móvil" };
#if WINDOWS
        // Ancho suficiente para que las 4 pestañas se vean sin el menú "···"
        ventana.Width = 1000;
        ventana.Height = 780;
        ventana.MinimumWidth = 720;
        ventana.MinimumHeight = 560;
#endif
        return ventana;
    }

    /// <summary>Después de iniciar sesión se muestra la app con pestañas.</summary>
    public void IrAInicio() => Windows[0].Page = new AppShell();

    public void CerrarSesion()
    {
        Sesion.Cerrar();
        Windows[0].Page = _servicios.GetRequiredService<LoginPage>();
    }
}
