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

    protected override Window CreateWindow(IActivationState? activationState) =>
        new(_servicios.GetRequiredService<LoginPage>()) { Title = "Biblioteca Móvil" };

    /// <summary>Después de iniciar sesión se muestra la app con pestañas.</summary>
    public void IrAInicio() => Windows[0].Page = new AppShell();

    public void CerrarSesion()
    {
        Sesion.Cerrar();
        Windows[0].Page = _servicios.GetRequiredService<LoginPage>();
    }
}
