# Biblioteca Móvil · .NET MAUI 10

Aplicación de biblioteca multiplataforma (Android y Windows) con **persistencia local en SQLite**, **patrón DAO** y las **7 actividades de diseño** de la unidad (VerticalStackLayout, HorizontalStackLayout, Grid, FlexLayout, AbsoluteLayout, BindableLayout y diseño personalizado).

## Cómo ejecutarla

1. Abre **Visual Studio 2026** con la carga de trabajo **Desarrollo de interfaz de usuario de aplicaciones multiplataforma de .NET (.NET MAUI)**.
2. *Archivo → Abrir → Proyecto o solución* y elige `BibliotecaMovil.csproj`.
3. Espera a que se restauren los paquetes NuGet (`sqlite-net-pcl` y `SQLitePCLRaw.bundle_green`).
4. En la barra superior elige el destino **Windows Machine** o un **emulador Android** y presiona **F5**.

También desde la terminal:

```bash
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```

### Cuentas de prueba

| Usuario  | Contraseña  | Rol           |
|----------|-------------|---------------|
| `admin`  | `admin123`  | Administrador |
| `alumno` | `alumno123` | Lector        |
| `mrojas` | `lector123` | Lector        |

La base `biblioteca.db3` se crea sola la primera vez, con 13 libros, 3 usuarios y 3 préstamos de ejemplo (uno vencido).

## Qué hace la app

| Pantalla  | Administrador | Lector |
|-----------|---------------|--------|
| Inicio    | Resumen, accesos rápidos, reporte, autores destacados y libros recientes | Igual, sin las opciones de administración |
| Catálogo  | Buscar, filtrar, registrar, editar y eliminar libros | Buscar, filtrar y ver detalles |
| Préstamos | Registrar préstamos, devoluciones e historial | Ver solo sus préstamos |
| Usuarios  | Alta, edición, cambio de rol y contraseña, baja | Ver su perfil |

## Las 7 actividades de diseño

| # | Layout | Dónde se usa | Archivo |
|---|--------|--------------|---------|
| 1 | **VerticalStackLayout** | Formulario de inicio de sesión: logo, título, usuario, contraseña y botón apilados | `Views/LoginPage.xaml` |
| 2 | **HorizontalStackLayout** | Barra de acciones del catálogo (Nuevo / Lista), botones Guardar · Eliminar · Cancelar, selector Activos / Historial, plazo en días, fila de autores | `CatalogoPage.xaml`, `LibroFormPage.xaml`, `PrestamosPage.xaml`, `InicioPage.xaml` |
| 3 | **Grid** | Formularios con columnas `Auto,*` (libro, préstamo, usuario), resumen `*,*,*` en Inicio, estructura del catálogo en filas y cuadrícula de libros de 1 a 4 columnas según el ancho | `LibroFormPage.xaml`, `PrestamosPage.xaml`, `UsuariosPage.xaml`, `InicioPage.xaml`, `CatalogoPage.xaml` |
| 4 | **FlexLayout** | Chips de categorías (`Direction="Row"`, `Wrap="Wrap"`) que saltan de línea según el espacio | `CatalogoPage.xaml` |
| 5 | **AbsoluteLayout** | Fondo del login con capas superpuestas (`LayoutBounds` y `LayoutFlags`), insignias **Nuevo** y **Disponible/Prestado** encima de cada portada, vista previa de la portada | `LoginPage.xaml`, `CatalogoPage.xaml`, `LibroFormPage.xaml` |
| 6 | **BindableLayout** | Chips de categorías, autores destacados, libros recientes y lista de usuarios generados desde colecciones (`BindableLayout.SetItemsSource` + `ItemTemplate`) | `CatalogoPage.xaml`, `InicioPage.xaml`, `UsuariosPage.xaml` |
| 7 | **Diseño personalizado** | `MosaicoLayout`: un layout propio (`Layout` + `ILayoutManager`) que alterna una celda destacada a lo ancho con 2 celdas normales. Además, estilos globales, colores y tema claro/oscuro | `Controls/MosaicoLayout.cs`, `Resources/Styles/Styles.xaml`, `Resources/Styles/Colors.xaml` |

Como recomienda la guía, las listas largas (catálogo y préstamos) usan **CollectionView**, y BindableLayout se reserva para colecciones pequeñas.

### Detalle de la Actividad 7: `MosaicoLayout`

```csharp
public class MosaicoLayout : Layout
{
    protected override ILayoutManager CreateLayoutManager() => new MosaicoLayoutManager(this);
}
```

`MosaicoLayoutManager` implementa:

- `Measure(...)`: calcula el tamaño de cada celda y el alto total.
- `ArrangeChildren(...)`: coloca cada hijo en su celda.

Los hijos ocultos (`IsVisible="False"`) no ocupan lugar, así que el mosaico se reacomoda cuando un lector no ve las opciones de administrador. Las propiedades `Columnas`, `Espacio` y `AltoCelda` se pueden cambiar desde XAML.

## Arquitectura (persistencia y acceso a datos)

```
Vista (Views/*.xaml)  →  DAO (Data/*DAO.cs)  →  SQLite (biblioteca.db3)
                          ↑ interfaces en Data/Interfaces.cs
```

- **Modelo / ORM**: `Models/Libro.cs`, `Usuario.cs` y `Prestamo.cs`. Los atributos `[Table]`, `[PrimaryKey]`, `[Indexed]` y `[Unique]` de sqlite-net crean las tablas e índices.
- **DAO**: `ILibroDAO`, `IUsuarioDAO` e `IPrestamoDAO`, con sus implementaciones. Las pantallas reciben el DAO por inyección de dependencias (`MauiProgram.cs`).
- **Consultas optimizadas**: son parametrizadas (evitan SQL Injection), usan `WHERE`, `ORDER BY` y `LIMIT`, proyectan solo las columnas necesarias (autores, JOIN de préstamos) y aprovechan índices en `Titulo`, `Categoria`, `IdLibro`, `IdUsuario` y un índice compuesto en `(FechaDevolucion, FechaLimite)`.
- **Transacciones**: registrar un préstamo, devolverlo o eliminar un libro o usuario son operaciones atómicas (`RunInTransactionAsync`).
- **Seguridad**: las contraseñas se guardan con hash PBKDF2-SHA256 y sal aleatoria, nunca en texto plano. El acceso se controla por rol (Administrador / Lector).

## Estructura

```
BibliotecaMovil/
├── App.xaml / AppShell.xaml / MauiProgram.cs
├── Controls/MosaicoLayout.cs          ← Actividad 7
├── Data/                              ← BD + DAO
├── Helpers/                           ← sesión, hash, paleta
├── Models/                            ← entidades
├── Resources/Styles/                  ← Actividad 7 (estilos)
├── Views/                             ← pantallas (Actividades 1–6)
└── Platforms/Android, Platforms/Windows
```
