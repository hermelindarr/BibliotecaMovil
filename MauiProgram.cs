using Microsoft.Extensions.Logging;

namespace BibliotecaMovil;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Capa de persistencia (una sola conexión SQLite para toda la app)
        builder.Services.AddSingleton<BibliotecaDatabase>();

        // Capa de acceso a datos (DAO)
        builder.Services.AddSingleton<ILibroDAO, LibroDAO>();
        builder.Services.AddSingleton<IUsuarioDAO, UsuarioDAO>();
        builder.Services.AddSingleton<IPrestamoDAO, PrestamoDAO>();

        // Capa de presentación (pantallas)
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<InicioPage>();
        builder.Services.AddTransient<CatalogoPage>();
        builder.Services.AddTransient<LibroFormPage>();
        builder.Services.AddTransient<PrestamosPage>();
        builder.Services.AddTransient<UsuariosPage>();

        return builder.Build();
    }
}
