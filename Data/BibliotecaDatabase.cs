using SQLite;

namespace BibliotecaMovil.Data;

/// <summary>
/// Fuente de datos: abre la base SQLite local (biblioteca.db3), crea las tablas
/// e índices la primera vez y carga datos de ejemplo.
/// </summary>
public class BibliotecaDatabase
{
    private const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

    private readonly SemaphoreSlim _candado = new(1, 1);
    private SQLiteAsyncConnection? _conexion;

    public static string Ruta => Path.Combine(FileSystem.AppDataDirectory, "biblioteca.db3");

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_conexion is not null) return _conexion;

        await _candado.WaitAsync();
        try
        {
            if (_conexion is null)
            {
                var db = new SQLiteAsyncConnection(Ruta, Flags);

                // ORM: crea las tablas a partir de las clases (atributos [Table], [Indexed]...)
                await db.CreateTablesAsync<Libro, Usuario, Prestamo>();

                // Índice compuesto para la consulta más frecuente: préstamos activos por fecha límite
                await db.ExecuteAsync(
                    "CREATE INDEX IF NOT EXISTS idx_prestamos_activos ON Prestamos (FechaDevolucion, FechaLimite)");

                await SembrarAsync(db);
                _conexion = db;
            }
        }
        finally
        {
            _candado.Release();
        }

        return _conexion;
    }

    private static async Task SembrarAsync(SQLiteAsyncConnection db)
    {
        if (await db.Table<Usuario>().CountAsync() == 0)
        {
            await db.InsertAllAsync(new[]
            {
                CrearUsuario("Administrador", "admin", "admin@biblioteca.mx", Roles.Administrador, "admin123"),
                CrearUsuario("Ana Torres", "alumno", "ana.torres@biblioteca.mx", Roles.Lector, "alumno123"),
                CrearUsuario("Miguel Rojas", "mrojas", "miguel.rojas@biblioteca.mx", Roles.Lector, "lector123"),
            });
        }

        if (await db.Table<Libro>().CountAsync() > 0) return;

        var hoy = DateTime.Now;
        var libros = new List<Libro>
        {
            new() { Titulo = "C# 12 in a Nutshell", Autor = "Joseph Albahari", Categoria = "Programación", Anio = 2023, FechaAlta = hoy.AddDays(-3) },
            new() { Titulo = "Código limpio", Autor = "Robert C. Martin", Categoria = "Programación", Anio = 2012, FechaAlta = hoy.AddDays(-90) },
            new() { Titulo = "Fundamentos de Bases de Datos", Autor = "Abraham Silberschatz", Categoria = "Bases de Datos", Anio = 2014, FechaAlta = hoy.AddDays(-120) },
            new() { Titulo = "Introducción a los Sistemas de Bases de Datos", Autor = "C. J. Date", Categoria = "Bases de Datos", Anio = 2001, FechaAlta = hoy.AddDays(-200) },
            new() { Titulo = "Redes de Computadoras", Autor = "Andrew S. Tanenbaum", Categoria = "Redes", Anio = 2012, FechaAlta = hoy.AddDays(-60) },
            new() { Titulo = "Sistemas Operativos Modernos", Autor = "Andrew S. Tanenbaum", Categoria = "Tecnología", Anio = 2015, FechaAlta = hoy.AddDays(-45), Disponible = false },
            new() { Titulo = "Organización de Computadoras", Autor = "Andrew S. Tanenbaum", Categoria = "Tecnología", Anio = 2000, FechaAlta = hoy.AddDays(-10) },
            new() { Titulo = "Scrum: el arte de hacer el doble", Autor = "Jeff Sutherland", Categoria = "Tecnología", Anio = 2014, FechaAlta = hoy.AddDays(-15) },
            new() { Titulo = "Breve historia del tiempo", Autor = "Stephen Hawking", Categoria = "Ciencia", Anio = 1988, FechaAlta = hoy.AddDays(-300) },
            new() { Titulo = "Física Universitaria", Autor = "Sears y Zemansky", Categoria = "Ciencia", Anio = 2018, FechaAlta = hoy.AddDays(-150) },
            new() { Titulo = "Cien años de soledad", Autor = "Gabriel García Márquez", Categoria = "Novela", Anio = 1967, FechaAlta = hoy.AddDays(-400) },
            new() { Titulo = "El principito", Autor = "Antoine de Saint-Exupéry", Categoria = "Novela", Anio = 1943, FechaAlta = hoy.AddDays(-7) },
            new() { Titulo = "Sapiens", Autor = "Yuval Noah Harari", Categoria = "Historia", Anio = 2011, FechaAlta = hoy.AddDays(-80), Disponible = false },
        };
        await db.InsertAllAsync(libros); // sqlite-net asigna el Id autoincremental a cada objeto

        var ana = await db.Table<Usuario>().Where(u => u.NombreUsuario == "alumno").FirstAsync();
        var miguel = await db.Table<Usuario>().Where(u => u.NombreUsuario == "mrojas").FirstAsync();

        await db.InsertAllAsync(new[]
        {
            new Prestamo { IdLibro = libros[5].Id, IdUsuario = ana.Id, FechaPrestamo = hoy.AddDays(-2), FechaLimite = hoy.AddDays(5) },
            new Prestamo { IdLibro = libros[12].Id, IdUsuario = miguel.Id, FechaPrestamo = hoy.AddDays(-9), FechaLimite = hoy.AddDays(-2) },
            new Prestamo { IdLibro = libros[10].Id, IdUsuario = ana.Id, FechaPrestamo = hoy.AddDays(-30), FechaLimite = hoy.AddDays(-23), FechaDevolucion = hoy.AddDays(-24) },
        });
    }

    private static Usuario CrearUsuario(string nombre, string usuario, string correo, string rol, string password)
    {
        var (hash, sal) = PasswordHasher.Crear(password);
        return new Usuario
        {
            Nombre = nombre,
            NombreUsuario = usuario,
            Correo = correo,
            Rol = rol,
            PasswordHash = hash,
            Salt = sal,
        };
    }
}
