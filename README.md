# 📖 Biblioteca Móvil · .NET MAUI 10

[![GitHub Pages](https://img.shields.io/badge/Demo_Web-GitHub_Pages-2563eb?style=for-the-badge&logo=github)](https://hermelindarr.github.io/BibliotecaMovil/)
[![.NET MAUI](https://img.shields.io/badge/.NET_MAUI-10.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![SQLite](https://img.shields.io/badge/SQLite-3-003B57?style=for-the-badge&logo=sqlite)](https://sqlite.org/)
[![C#](https://img.shields.io/badge/C%23-12-239120?style=for-the-badge&logo=csharp)](https://learn.microsoft.com/dotnet/csharp/)
[![Universidad](https://img.shields.io/badge/UTSJR-Unidad_Académica_Jalpan-1e3a8a?style=for-the-badge)](https://utsjr.edu.mx/)

Aplicación multiplataforma de gestión bibliotecaria desarrollada para **Android y Windows** en **.NET MAUI 10**, con **persistencia local en SQLite**, **patrón DAO** y el cumplimiento integral de las **7 actividades de diseño de interfaces** de la Unidad 3.

🌐 **Sitio Web & Simulador en Vivo:** [https://hermelindarr.github.io/BibliotecaMovil/](https://hermelindarr.github.io/BibliotecaMovil/)

---

## 🚀 Cómo ejecutar la aplicación

### Opción 1: Visual Studio 2026 (o Visual Studio 2022 v17.10+)
1. Abre Visual Studio con la carga de trabajo **Desarrollo de interfaz de usuario de aplicaciones multiplataforma de .NET (.NET MAUI)** instalada.
2. Abre el archivo de solución o proyecto: `BibliotecaMovil.csproj`.
3. Espera a que los paquetes NuGet (`sqlite-net-pcl` y `SQLitePCLRaw.bundle_green`) se restauren automáticamente.
4. En el selector superior de destinos de depuración:
   - **Windows:** Selecciona `Windows Machine` y presiona **F5**.
   - **Android:** Selecciona tu emulador Android (Pixel con API 31+) o dispositivo físico y presiona **F5**.

### Opción 2: Terminal (.NET CLI)
```bash
# Compilar y ejecutar directamente en Windows
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```

---

## 👥 Cuentas de Acceso Preconfiguradas

| Usuario | Contraseña | Rol | Permisos |
| :--- | :--- | :--- | :--- |
| `admin` | `admin123` | **Administrador** | CRUD completo de libros, préstamos, devoluciones, historial y gestión de usuarios. |
| `alumno` | `alumno123` | **Lector** | Búsqueda y filtrado en el catálogo, consulta de disponibilidad y préstamos personales. |
| `mrojas` | `lector123` | **Lector** | Consulta de catálogo y préstamos personales. |

> La base de datos local `biblioteca.db3` se inicializa y siembra automáticamente en el primer inicio con 13 libros, 3 usuarios y 3 préstamos de prueba.

---

## 🎨 Las 7 Actividades de Diseño (.NET MAUI Layouts)

Cumplimiento riguroso de la guía práctica de diseño multiplataforma:

| # | Layout | Dónde se implementa | Archivo fuente |
|---|--------|---------------------|----------------|
| **1** | **VerticalStackLayout** | Pantalla de inicio de sesión lineal: Logo, títulos descriptivos, formulario de credenciales y botones apilados verticalmente con espaciado uniforme. | [`Views/LoginPage.xaml`](file:///Views/LoginPage.xaml) |
| **2** | **HorizontalStackLayout** | Barra de acciones del catálogo (+ Nuevo libro, Lista/Cuadrícula, conteo), botones de acción (Guardar, Cancelar, Devolver) y fila de autores. | [`Views/CatalogoPage.xaml`](file:///Views/CatalogoPage.xaml), [`Views/LibroFormPage.xaml`](file:///Views/LibroFormPage.xaml) |
| **3** | **Grid** | Formulario de captura estructurado con columnas `Auto,*` para etiquetas y campos, y panel de métricas en Inicio con `*,*,*`. | [`Views/LibroFormPage.xaml`](file:///Views/LibroFormPage.xaml), [`Views/InicioPage.xaml`](file:///Views/InicioPage.xaml) |
| **4** | **FlexLayout** | Filtro de categorías dinámico con envoltorio (`Direction="Row"`, `Wrap="Wrap"`), adaptándose a cualquier ancho de pantalla móvil o de escritorio. | [`Views/CatalogoPage.xaml`](file:///Views/CatalogoPage.xaml) |
| **5** | **AbsoluteLayout** | Fondo decorativo de inicio de sesión con capas superpuestas, círculos flotantes e insignias dinámicas ("Nuevo", "Disponible") superpuestas sobre las portadas de los libros. | [`Views/LoginPage.xaml`](file:///Views/LoginPage.xaml), [`Views/CatalogoPage.xaml`](file:///Views/CatalogoPage.xaml) |
| **6** | **BindableLayout** | Enlace y renderizado de colecciones dinámicas (chips de categorías, autores destacados) sin la sobrecarga pesada de un CollectionView. | [`Views/CatalogoPage.xaml`](file:///Views/CatalogoPage.xaml), [`Views/InicioPage.xaml`](file:///Views/InicioPage.xaml) |
| **7** | **Diseño Personalizado (Custom Layout) & Estilos** | `MosaicoLayout`: contenedor propio que hereda de `Layout` e implementa `ILayoutManager` (`Measure` y `ArrangeChildren`) para alternar celdas destacadas y secundarias. Además de recursos globales y tema claro/oscuro. | [`Controls/MosaicoLayout.cs`](file:///Controls/MosaicoLayout.cs), [`Resources/Styles/Styles.xaml`](file:///Resources/Styles/Styles.xaml) |

---

## 🗄️ Persistencia y Acceso a Datos (Unidad 3)

### Arquitectura en Capas
```
┌──────────────────────────────────────────────┐
│                  VISTAS UI                   │
│   LoginPage · InicioPage · CatalogoPage      │
│   LibroFormPage · PrestamosPage              │
└──────────────────────┬───────────────────────┘
                       │ Inyección de dependencias
┌──────────────────────▼───────────────────────┐
│              CAPA DAO (CONTRATOS)            │
│   ILibroDAO · IPrestamoDAO · IUsuarioDAO     │
└──────────────────────┬───────────────────────┘
                       │ Implementación asíncrona
┌──────────────────────▼───────────────────────┐
│           ORM & BASE DE DATOS LOCAL          │
│   SQLiteAsyncConnection (biblioteca.db3)     │
│   Tablas: Libros, Usuarios, Prestamos        │
└──────────────────────────────────────────────┘
```

- **Mapeo Objeto-Relacional (ORM):** Modelado declarativo mediante atributos `[Table]`, `[PrimaryKey, AutoIncrement]`, `[Indexed]` y `[MaxLength]`.
- **Transacciones Atómicas:** El registro y devolución de préstamos utiliza transacciones (`RunInTransactionAsync`) para asegurar la consistencia entre el cambio de estado del libro (`Disponible = false/true`) y el registro del préstamo.
- **Consultas Optimizadas:** Creación programática de índices compuestos como `CREATE INDEX idx_prestamos_activos ON Prestamos (FechaDevolucion, FechaLimite)`, consultas parametrizadas contra SQL Injection y proyecciones selectivas.
- **Seguridad Criptográfica:** Hashing seguro de contraseñas mediante **PBKDF2-SHA256** con sal generada de forma criptográfica (`PasswordHasher.cs`).

---

## 📁 Estructura del Proyecto

```
BibliotecaMovil/
├── Controls/
│   └── MosaicoLayout.cs          # Actividad 7: Custom Layout (Layout + ILayoutManager)
├── Data/
│   ├── BibliotecaDatabase.cs     # Inicialización de SQLite, tablas e índices
│   ├── Interfaces.cs             # Contratos DAO (ILibroDAO, IUsuarioDAO, IPrestamoDAO)
│   ├── LibroDAO.cs               # Implementación CRUD y consultas optimizadas
│   ├── PrestamoDAO.cs            # Transacciones de préstamos y devoluciones
│   └── UsuarioDAO.cs             # Autenticación y gestión de usuarios
├── Helpers/
│   ├── ObservableObject.cs       # Soporte INotifyPropertyChanged
│   ├── Paleta.cs                 # Colores del sistema
│   ├── PasswordHasher.cs         # Hashing criptográfico PBKDF2
│   └── Sesion.cs                 # Estado de sesión y control por roles
├── Models/
│   ├── Libro.cs                  # Entidad Libro
│   ├── Usuario.cs                # Entidad Usuario
│   ├── Prestamo.cs               # Entidad Préstamo
│   ├── Categorias.cs             # Catálogo de categorías
│   └── FiltroCategoria.cs        # Modelo para chips de filtro
├── Resources/
│   ├── AppIcon/                  # Iconografía
│   ├── Images/                   # Iconos SVG de navegación
│   └── Styles/                   # Actividad 7: Colores y estilos globales
├── Views/
│   ├── LoginPage.xaml            # Actividad 1 & 5 (VerticalStackLayout + AbsoluteLayout)
│   ├── InicioPage.xaml           # Actividad 3, 6 & 7 (Grid + BindableLayout + MosaicoLayout)
│   ├── CatalogoPage.xaml         # Actividad 2, 4, 5 & 6 (FlexLayout + Chips + Badges)
│   ├── LibroFormPage.xaml        # Actividad 2 & 3 (Grid Formulario Auto,*)
│   ├── PrestamosPage.xaml        # Gestión de préstamos
│   └── UsuariosPage.xaml         # Gestión de usuarios
├── index.html                    # Publicación oficial en GitHub Pages con Simulador
├── docs/index.html               # Espejo para configuración alternativa en GitHub Pages
├── .nojekyll                     # Configuración de despliegue estático para Pages
├── App.xaml / AppShell.xaml      # Navegación basada en Shell con TabBar
└── MauiProgram.cs                # Configuración e inyección de dependencias
```

---

## 🏛️ Institución
- **Universidad Tecnológica de San Juan del Río (UTSJR)**
- **Unidad Académica Jalpan**
- **Asignatura:** Desarrollo de Aplicaciones Móviles / Unidad 3
