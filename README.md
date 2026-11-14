# Práctica: Aplicación Biblioteca Móvil (.NET MAUI 10)

**Universidad Tecnológica de San Juan del Río (UTSJR) · Unidad Académica Jalpan**  
**Carrera:** Ingeniería en Desarrollo y Gestión de Software (IDyGS)  
**Materia:** Desarrollo de Aplicaciones Móviles  
**Unidad 3:** Persistencia y Acceso a Datos & Diseño de Interfaces  
**Alumna:** Hermelinda Rivera Reséndiz  

🌐 **Página web del proyecto en GitHub Pages:** [https://hermelindarr.github.io/BibliotecaMovil/](https://hermelindarr.github.io/BibliotecaMovil/)

---

## 📌 Descripción de la práctica

En esta práctica desarrollé una aplicación de **Biblioteca Móvil** utilizando **.NET MAUI 10** en C#. El objetivo principal fue aprender a implementar la **persistencia de datos local con SQLite**, aplicar el patrón de diseño **DAO** para separar la base de datos de las pantallas, y poner en práctica las **7 actividades de diseño** de interfaces que vimos en la unidad.

La aplicación permite:
- Iniciar sesión con control de roles (**Administrador** y **Lector**).
- Consultar un catálogo de libros con buscador por texto y filtros por categorías.
- Dar de alta, editar y eliminar libros (CRUD completo).
- Registrar préstamos de libros y devoluciones, cambiando automáticamente el estado a disponible o prestado.
- Consultar usuarios registrados.

---

## 🚀 Cómo abrir y ejecutar el proyecto

1. Descarga o clona este repositorio en tu computadora:
   ```bash
   git clone https://github.com/hermelindarr/BibliotecaMovil.git
   ```
2. Abre **Visual Studio 2026** (o Visual Studio 2022 con .NET MAUI instalado).
3. Selecciona *Archivo > Abrir > Proyecto o solución* y abre el archivo `BibliotecaMovil.csproj`.
4. Espera a que Visual Studio restaure los paquetes NuGet (`sqlite-net-pcl` y `SQLitePCLRaw.bundle_green`).
5. En la parte superior elige el dispositivo de prueba:
   - Para Windows: Selecciona **Windows Machine** y presiona **F5**.
   - Para Android: Selecciona un **Emulador Android** y presiona **F5**.

### Cuentas de prueba preconfiguradas:

| Usuario | Contraseña | Rol | Lo que puede hacer |
|---|---|---|---|
| `admin` | `admin123` | Administrador | Control total: registrar libros, editarlos, borrarlos, hacer préstamos y devoluciones. |
| `alumno` | `alumno123` | Lector | Consulta de libros en el catálogo y ver sus préstamos. |
| `mrojas` | `lector123` | Lector | Consulta de libros y préstamos. |

> Nota: La base de datos local `biblioteca.db3` se crea de forma automática en el primer inicio de la app con 13 libros y préstamos de ejemplo.

---

## 🎨 Las 7 Actividades de Diseño (.NET MAUI Layouts)

En el proyecto apliqué los 7 contenedores de diseño vistos en clase:

1. **VerticalStackLayout:** Lo utilicé en la pantalla de login (`Views/LoginPage.xaml`) para apilar hacia abajo el logo, los títulos, los campos de texto de usuario y contraseña, y el botón de entrar.
2. **HorizontalStackLayout:** Lo usé en la barra de botones del catálogo (`Views/CatalogoPage.xaml`) para colocar los botones de `+ Nuevo libro`, el botón de cambiar vista y el total de libros en una sola fila.
3. **Grid:** Lo apliqué en el formulario de registro de libros (`Views/LibroFormPage.xaml`) con `ColumnDefinitions="Auto,*"` para que las etiquetas queden alineadas a la izquierda y las cajas de texto ocupen el resto del ancho.
4. **FlexLayout:** Lo implementé en los chips de categorías del catálogo (`Views/CatalogoPage.xaml`) con `Wrap="Wrap"` para que los botones brinquen de renglón si la pantalla del celular es pequeña.
5. **AbsoluteLayout:** Lo usé en el fondo del login para superponer círculos decorativos y en las portadas de los libros para colocar la etiqueta flotante de **NUEVO** en la esquina superior.
6. **BindableLayout:** Lo utilicé en `CatalogoPage.xaml` para generar los botones de categorías dinámicamente desde una lista en C# sin tener que usar un CollectionView pesado.
7. **Diseño Personalizado (Custom Layout):** Programé mi propio contenedor en C# llamado `MosaicoLayout` en la carpeta `Controls/MosaicoLayout.cs` heredando de `Layout` e implementando `ILayoutManager`. Este layout acomoda los accesos directos en forma de mosaico (una tarjeta ancha y dos tarjetas chicas). Además personalicé los colores y estilos en `Resources/Styles/Styles.xaml`.

---

## 💾 Persistencia y Acceso a Datos (SQLite + DAO)

- **Base de datos SQLite:** Se utiliza `biblioteca.db3` con sqlite-net-pcl en modo asíncrono (`SQLiteAsyncConnection`).
- **Tablas:** Se definieron tres modelos en la carpeta `Models/`:
  - `Libro.cs`: id, título, autor, categoría, año, disponible.
  - `Usuario.cs`: id, nombre, usuario, correo, rol, hash de contraseña.
  - `Prestamo.cs`: id, idLibro, idUsuario, fechas de préstamo y devolución.
- **Patrón DAO:** Todo el acceso a la base de datos se realiza a través de las interfaces en `Data/Interfaces.cs` (`ILibroDAO`, `IPrestamoDAO`, `IUsuarioDAO`) y sus clases correspondientes en `Data/`. Esto hace que las páginas XAML no toquen directamente SQLite.
- **Optimización:** Se creó un índice compuesto `CREATE INDEX IF NOT EXISTS idx_prestamos_activos ON Prestamos (FechaDevolucion, FechaLimite)` para acelerar las consultas de libros prestados.
- **Transacciones:** Cuando se registra un préstamo o una devolución, se ejecuta una transacción atómica para asegurar que el libro cambie su estado a disponible/no disponible al mismo tiempo que se guarda el préstamo.

---

## 📁 Estructura del proyecto

```
BibliotecaMovil/
├── Controls/
│   └── MosaicoLayout.cs          # Actividad 7: Layout propio en C#
├── Data/
│   ├── BibliotecaDatabase.cs     # Conexión y creación de tablas SQLite
│   ├── Interfaces.cs             # Interfaces del patrón DAO
│   ├── LibroDAO.cs               # Consultas y CRUD de libros
│   ├── PrestamoDAO.cs            # Registro de préstamos y devoluciones
│   └── UsuarioDAO.cs             # Autenticación y usuarios
├── Helpers/
│   ├── ObservableObject.cs       # Notificaciones de cambio de datos
│   ├── PasswordHasher.cs         # Cifrado de contraseñas con PBKDF2
│   └── Sesion.cs                 # Control de rol de usuario actual
├── Models/
│   ├── Libro.cs
│   ├── Usuario.cs
│   ├── Prestamo.cs
│   └── Categorias.cs
├── Resources/
│   ├── AppIcon/                  # Icono de la app
│   ├── Images/                   # Iconos de las pestañas
│   └── Styles/                   # Colores y estilos globales
├── Views/
│   ├── LoginPage.xaml            # Login (VerticalStackLayout y AbsoluteLayout)
│   ├── InicioPage.xaml           # Inicio (MosaicoLayout y Grid)
│   ├── CatalogoPage.xaml         # Catálogo (FlexLayout, HorizontalStackLayout)
│   ├── LibroFormPage.xaml        # Formulario (Grid)
│   ├── PrestamosPage.xaml        # Préstamos
│   └── UsuariosPage.xaml         # Usuarios
├── index.html                    # Página para GitHub Pages
├── docs/index.html               # Respaldo para GitHub Pages
├── App.xaml / AppShell.xaml      # Navegación con pestañas
└── MauiProgram.cs                # Inicialización de servicios y DAOs
```
