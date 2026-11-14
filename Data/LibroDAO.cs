namespace BibliotecaMovil.Data;

public class LibroDAO(BibliotecaDatabase bd) : ILibroDAO
{
    public async Task<List<Libro>> BuscarAsync(string? texto, string? categoria, int limite = 200)
    {
        var db = await bd.GetConnectionAsync();
        var patron = $"%{texto?.Trim()}%";
        var cat = categoria ?? string.Empty;

        // Consulta parametrizada (evita SQL Injection), filtrada, ordenada y limitada
        return await db.QueryAsync<Libro>(
            """
            SELECT * FROM Libros
            WHERE (? = '' OR Categoria = ?)
              AND (Titulo LIKE ? OR Autor LIKE ?)
            ORDER BY Titulo
            LIMIT ?
            """,
            cat, cat, patron, patron, limite);
    }

    public async Task<List<Libro>> DisponiblesAsync()
    {
        var db = await bd.GetConnectionAsync();
        return await db.QueryAsync<Libro>("SELECT * FROM Libros WHERE Disponible = 1 ORDER BY Titulo");
    }

    public async Task<List<Libro>> RecientesAsync(int cantidad)
    {
        var db = await bd.GetConnectionAsync();
        return await db.QueryAsync<Libro>("SELECT * FROM Libros ORDER BY FechaAlta DESC LIMIT ?", cantidad);
    }

    public async Task<List<AutorResumen>> AutoresDestacadosAsync(int cantidad)
    {
        var db = await bd.GetConnectionAsync();
        // Proyección: solo las dos columnas que necesita la pantalla
        return await db.QueryAsync<AutorResumen>(
            "SELECT Autor, COUNT(*) AS Libros FROM Libros GROUP BY Autor ORDER BY Libros DESC, Autor LIMIT ?",
            cantidad);
    }

    public async Task<Libro?> ObtenerPorIdAsync(int id)
    {
        var db = await bd.GetConnectionAsync();
        return await db.FindAsync<Libro>(id);
    }

    public async Task<(int Total, int Disponibles)> ContarAsync()
    {
        var db = await bd.GetConnectionAsync();
        var total = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Libros");
        var disponibles = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Libros WHERE Disponible = 1");
        return (total, disponibles);
    }

    public async Task GuardarAsync(Libro libro)
    {
        var db = await bd.GetConnectionAsync();
        if (libro.Id == 0)
            await db.InsertAsync(libro);
        else
            await db.UpdateAsync(libro);
    }

    public async Task EliminarAsync(int id)
    {
        var db = await bd.GetConnectionAsync();
        var activos = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Prestamos WHERE IdLibro = ? AND FechaDevolucion IS NULL", id);
        if (activos > 0)
            throw new InvalidOperationException("Este libro está prestado. Registra la devolución antes de eliminarlo.");

        await db.RunInTransactionAsync(conn =>
        {
            conn.Execute("DELETE FROM Prestamos WHERE IdLibro = ?", id); // historial del libro
            conn.Delete<Libro>(id);
        });
    }
}
