namespace BibliotecaMovil.Data;

// Contratos de la capa de acceso a datos (patrón DAO).
// Las pantallas solo conocen estas interfaces, no SQLite.

public interface ILibroDAO
{
    Task<List<Libro>> BuscarAsync(string? texto, string? categoria, int limite = 200);
    Task<List<Libro>> DisponiblesAsync();
    Task<List<Libro>> RecientesAsync(int cantidad);
    Task<List<AutorResumen>> AutoresDestacadosAsync(int cantidad);
    Task<Libro?> ObtenerPorIdAsync(int id);
    Task<(int Total, int Disponibles)> ContarAsync();
    Task GuardarAsync(Libro libro);
    Task EliminarAsync(int id);
}

public interface IUsuarioDAO
{
    Task<List<Usuario>> ObtenerTodosAsync();
    Task<Usuario?> AutenticarAsync(string nombreUsuario, string password);
    Task GuardarAsync(Usuario usuario, string? passwordNueva);
    Task EliminarAsync(int id);
}

public interface IPrestamoDAO
{
    Task<List<PrestamoDetalle>> ObtenerActivosAsync(int? idUsuario = null);
    Task<List<PrestamoDetalle>> HistorialAsync(int? idUsuario = null, int limite = 50);
    Task<int> ContarActivosAsync(int? idUsuario = null);
    Task<int> ContarVencidosAsync();
    Task RegistrarAsync(int idLibro, int idUsuario, int dias);
    Task DevolverAsync(int idPrestamo);
}
