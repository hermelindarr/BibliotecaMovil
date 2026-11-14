namespace BibliotecaMovil.Data;

public class PrestamoDAO(BibliotecaDatabase bd) : IPrestamoDAO
{
    // JOIN de las tres tablas, solo con las columnas que muestra la pantalla
    private const string SelectDetalle =
        """
        SELECT p.Id, p.IdLibro, p.IdUsuario, p.FechaPrestamo, p.FechaLimite, p.FechaDevolucion,
               l.Titulo AS TituloLibro, l.Categoria AS Categoria, u.Nombre AS NombreUsuario
        FROM Prestamos p
        INNER JOIN Libros l ON l.Id = p.IdLibro
        INNER JOIN Usuarios u ON u.Id = p.IdUsuario
        """;

    public async Task<List<PrestamoDetalle>> ObtenerActivosAsync(int? idUsuario = null)
    {
        var db = await bd.GetConnectionAsync();
        var id = idUsuario ?? 0;
        return await db.QueryAsync<PrestamoDetalle>(
            $"""
            {SelectDetalle}
            WHERE p.FechaDevolucion IS NULL AND (? = 0 OR p.IdUsuario = ?)
            ORDER BY p.FechaLimite
            LIMIT 200
            """,
            id, id);
    }

    public async Task<List<PrestamoDetalle>> HistorialAsync(int? idUsuario = null, int limite = 50)
    {
        var db = await bd.GetConnectionAsync();
        var id = idUsuario ?? 0;
        return await db.QueryAsync<PrestamoDetalle>(
            $"""
            {SelectDetalle}
            WHERE p.FechaDevolucion IS NOT NULL AND (? = 0 OR p.IdUsuario = ?)
            ORDER BY p.FechaDevolucion DESC
            LIMIT ?
            """,
            id, id, limite);
    }

    public async Task<int> ContarActivosAsync(int? idUsuario = null)
    {
        var db = await bd.GetConnectionAsync();
        var id = idUsuario ?? 0;
        return await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Prestamos WHERE FechaDevolucion IS NULL AND (? = 0 OR IdUsuario = ?)", id, id);
    }

    public async Task<int> ContarVencidosAsync()
    {
        var db = await bd.GetConnectionAsync();
        // Las fechas se guardan como ticks: se compara contra el inicio del día de hoy
        return await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Prestamos WHERE FechaDevolucion IS NULL AND FechaLimite < ?",
            DateTime.Today.Ticks);
    }

    public async Task RegistrarAsync(int idLibro, int idUsuario, int dias)
    {
        var db = await bd.GetConnectionAsync();

        // Transacción: el préstamo y el cambio de disponibilidad se guardan juntos o no se guarda nada
        await db.RunInTransactionAsync(conn =>
        {
            var disponible = conn.ExecuteScalar<int>("SELECT Disponible FROM Libros WHERE Id = ?", idLibro);
            if (disponible == 0)
                throw new InvalidOperationException("Ese libro ya está prestado.");

            conn.Insert(new Prestamo
            {
                IdLibro = idLibro,
                IdUsuario = idUsuario,
                FechaPrestamo = DateTime.Now,
                FechaLimite = DateTime.Today.AddDays(dias),
            });
            conn.Execute("UPDATE Libros SET Disponible = 0 WHERE Id = ?", idLibro);
        });
    }

    public async Task DevolverAsync(int idPrestamo)
    {
        var db = await bd.GetConnectionAsync();
        await db.RunInTransactionAsync(conn =>
        {
            var prestamo = conn.Find<Prestamo>(idPrestamo)
                           ?? throw new InvalidOperationException("El préstamo ya no existe.");
            if (prestamo.FechaDevolucion is not null) return;

            prestamo.FechaDevolucion = DateTime.Now;
            conn.Update(prestamo);
            conn.Execute("UPDATE Libros SET Disponible = 1 WHERE Id = ?", prestamo.IdLibro);
        });
    }
}
