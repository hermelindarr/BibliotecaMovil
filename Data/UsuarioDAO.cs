namespace BibliotecaMovil.Data;

public class UsuarioDAO(BibliotecaDatabase bd) : IUsuarioDAO
{
    public async Task<List<Usuario>> ObtenerTodosAsync()
    {
        var db = await bd.GetConnectionAsync();
        return await db.Table<Usuario>().OrderBy(u => u.Nombre).ToListAsync();
    }

    public async Task<Usuario?> AutenticarAsync(string nombreUsuario, string password)
    {
        var db = await bd.GetConnectionAsync();
        var clave = nombreUsuario.Trim().ToLowerInvariant();
        var usuario = await db.Table<Usuario>().Where(u => u.NombreUsuario == clave).FirstOrDefaultAsync();

        if (usuario is null) return null;
        var valida = await Task.Run(() => PasswordHasher.Verificar(password, usuario.PasswordHash, usuario.Salt));
        return valida ? usuario : null;
    }

    public async Task GuardarAsync(Usuario usuario, string? passwordNueva)
    {
        var db = await bd.GetConnectionAsync();
        usuario.NombreUsuario = usuario.NombreUsuario.Trim().ToLowerInvariant();

        var nombre = usuario.NombreUsuario;
        var id = usuario.Id;
        var repetido = await db.Table<Usuario>().Where(u => u.NombreUsuario == nombre && u.Id != id).CountAsync();
        if (repetido > 0)
            throw new InvalidOperationException($"El usuario \"{nombre}\" ya existe.");

        if (!string.IsNullOrEmpty(passwordNueva))
        {
            (usuario.PasswordHash, usuario.Salt) = await Task.Run(() => PasswordHasher.Crear(passwordNueva));
        }
        else if (usuario.Id == 0)
        {
            throw new InvalidOperationException("La contraseña es obligatoria para un usuario nuevo.");
        }
        else
        {
            // Se conserva la contraseña actual
            var actual = await db.FindAsync<Usuario>(usuario.Id)
                         ?? throw new InvalidOperationException("El usuario ya no existe.");
            usuario.PasswordHash = actual.PasswordHash;
            usuario.Salt = actual.Salt;
        }

        if (usuario.Id == 0)
            await db.InsertAsync(usuario);
        else
            await db.UpdateAsync(usuario);
    }

    public async Task EliminarAsync(int id)
    {
        var db = await bd.GetConnectionAsync();
        var activos = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Prestamos WHERE IdUsuario = ? AND FechaDevolucion IS NULL", id);
        if (activos > 0)
            throw new InvalidOperationException("El usuario tiene libros prestados. Registra la devolución primero.");

        await db.RunInTransactionAsync(conn =>
        {
            conn.Execute("DELETE FROM Prestamos WHERE IdUsuario = ?", id);
            conn.Delete<Usuario>(id);
        });
    }
}
