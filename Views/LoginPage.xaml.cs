namespace BibliotecaMovil.Views;

public partial class LoginPage : ContentPage
{
    private readonly IUsuarioDAO _usuarios;

    public LoginPage(IUsuarioDAO usuarios)
    {
        InitializeComponent();
        _usuarios = usuarios;
    }

    private void OnUsuarioCompletado(object? sender, EventArgs e) => EntPassword.Focus();

    private void OnVerPassword(object? sender, EventArgs e)
    {
        EntPassword.IsPassword = !EntPassword.IsPassword;
        BtnVer.Text = EntPassword.IsPassword ? "Ver" : "Ocultar";
    }

    private async void OnIniciarSesion(object? sender, EventArgs e)
    {
        LblError.IsVisible = false;
        var usuario = EntUsuario.Text?.Trim() ?? string.Empty;
        var password = EntPassword.Text ?? string.Empty;

        if (usuario.Length == 0 || password.Length == 0)
        {
            MostrarError("Escribe tu usuario y tu contraseña.");
            return;
        }

        Ocupado(true);
        try
        {
            var encontrado = await _usuarios.AutenticarAsync(usuario, password);
            if (encontrado is null)
            {
                MostrarError("Usuario o contraseña incorrectos.");
                return;
            }

            Sesion.Iniciar(encontrado);
            EntPassword.Text = string.Empty;
            ((App)Application.Current!).IrAInicio();
        }
        catch (Exception ex)
        {
            MostrarError($"No se pudo iniciar sesión: {ex.Message}");
        }
        finally
        {
            Ocupado(false);
        }
    }

    private void MostrarError(string mensaje)
    {
        LblError.Text = mensaje;
        LblError.IsVisible = true;
    }

    private void Ocupado(bool ocupado)
    {
        BtnEntrar.IsEnabled = !ocupado;
        Cargando.IsRunning = Cargando.IsVisible = ocupado;
    }
}
