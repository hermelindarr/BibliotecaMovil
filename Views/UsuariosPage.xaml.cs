using System.Text.RegularExpressions;

namespace BibliotecaMovil.Views;

public partial class UsuariosPage : ContentPage
{
    private static readonly Regex FormatoUsuario = new("^[a-z0-9._]{3,30}$");

    private readonly IUsuarioDAO _usuarios;
    private Usuario? _enEdicion;

    public UsuariosPage(IUsuarioDAO usuarios)
    {
        InitializeComponent();
        _usuarios = usuarios;
        PkrRol.ItemsSource = Roles.Todos.ToList();
        PkrRol.SelectedItem = Roles.Lector;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var admin = Sesion.EsAdmin;
        PanelAdmin.IsVisible = admin;
        PanelPerfil.IsVisible = !admin;
        Title = admin ? "Usuarios" : "Mi perfil";

        if (!admin)
        {
            var yo = Sesion.Actual;
            LblPerfilIniciales.Text = yo?.Iniciales;
            LblPerfilNombre.Text = yo?.Nombre;
            LblPerfilDetalle.Text = yo is null ? string.Empty : $"{yo.Descripcion}\n{yo.Correo}";
            return;
        }

        await CargarAsync();
    }

    private async Task CargarAsync()
    {
        try
        {
            BindableLayout.SetItemsSource(ListaUsuarios, await _usuarios.ObtenerTodosAsync());
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("No se pudieron cargar los usuarios", ex.Message, "Aceptar");
        }
    }

    private async void OnGuardar(object? sender, EventArgs e)
    {
        var nombre = EntNombre.Text?.Trim() ?? string.Empty;
        var usuario = EntUsuario.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        var correo = EntCorreo.Text?.Trim() ?? string.Empty;
        var rol = PkrRol.SelectedItem as string;
        var password = EntPassword.Text ?? string.Empty;
        var esNuevo = _enEdicion is null;

        var error =
            nombre.Length == 0 ? "Escribe el nombre." :
            !FormatoUsuario.IsMatch(usuario) ? "El usuario debe tener de 3 a 30 caracteres: letras minúsculas, números, punto o guion bajo." :
            correo.Length > 0 && !(correo.Contains('@') && correo.Contains('.')) ? "Escribe un correo válido o déjalo vacío." :
            rol is null ? "Elige un rol." :
            esNuevo && password.Length < 6 ? "La contraseña debe tener al menos 6 caracteres." :
            !esNuevo && password.Length is > 0 and < 6 ? "La nueva contraseña debe tener al menos 6 caracteres." :
            _enEdicion?.Id == Sesion.Actual?.Id && rol != Roles.Administrador ? "No puedes quitarte el rol de Administrador a ti mismo." :
            null;

        LblError.Text = error;
        LblError.IsVisible = error is not null;
        if (error is not null) return;

        var entidad = new Usuario
        {
            Id = _enEdicion?.Id ?? 0,
            Nombre = nombre,
            NombreUsuario = usuario,
            Correo = correo,
            Rol = rol!,
        };

        BtnGuardar.IsEnabled = false;
        try
        {
            await _usuarios.GuardarAsync(entidad, password.Length > 0 ? password : null);
            if (entidad.Id == Sesion.Actual?.Id) Sesion.Iniciar(entidad);
            Limpiar();
            await CargarAsync();
        }
        catch (InvalidOperationException ex)
        {
            LblError.Text = ex.Message;
            LblError.IsVisible = true;
        }
        finally
        {
            BtnGuardar.IsEnabled = true;
        }
    }

    private void OnEditar(object? sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is not Usuario usuario) return;

        _enEdicion = usuario;
        LblFormTitulo.Text = $"Editar a {usuario.Nombre}";
        BtnGuardar.Text = "Guardar cambios";
        EntNombre.Text = usuario.Nombre;
        EntUsuario.Text = usuario.NombreUsuario;
        EntCorreo.Text = usuario.Correo;
        PkrRol.SelectedItem = usuario.Rol;
        EntPassword.Text = string.Empty;
        EntPassword.Placeholder = "Déjala vacía para no cambiarla";
        LblError.IsVisible = false;
        EntNombre.Focus();
    }

    private async void OnEliminar(object? sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is not Usuario usuario) return;

        if (usuario.Id == Sesion.Actual?.Id)
        {
            await DisplayAlertAsync("No permitido", "No puedes borrar tu propia cuenta mientras la usas.", "Aceptar");
            return;
        }

        var confirmar = await DisplayAlertAsync(
            "Borrar usuario",
            $"Se borrará a {usuario.Nombre} y su historial de préstamos. Esta acción no se puede deshacer.",
            "Borrar", "Cancelar");
        if (!confirmar) return;

        try
        {
            await _usuarios.EliminarAsync(usuario.Id);
            if (_enEdicion?.Id == usuario.Id) Limpiar();
            await CargarAsync();
        }
        catch (InvalidOperationException ex)
        {
            await DisplayAlertAsync("No se puede borrar", ex.Message, "Aceptar");
        }
    }

    private void OnLimpiar(object? sender, EventArgs e) => Limpiar();

    private void Limpiar()
    {
        _enEdicion = null;
        LblFormTitulo.Text = "Nuevo usuario";
        BtnGuardar.Text = "Guardar usuario";
        EntNombre.Text = EntUsuario.Text = EntCorreo.Text = EntPassword.Text = string.Empty;
        EntPassword.Placeholder = "Mínimo 6 caracteres";
        PkrRol.SelectedItem = Roles.Lector;
        LblError.IsVisible = false;
    }
}
