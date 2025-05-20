# Campus Love 💘

¡Hola! Bienvenido a **Campus Love**.


## ¿Qué es Campus Love? 📱

Campus Love es una aplicación de consola que simula un sistema de citas,
la app permite a las personas registrarse, ver perfiles de otros usuarios según diferentes criterios de compatibilidad (intereses comunes, proximidad geográfica o edad similar), dar likes/dislikes y encontrar coincidencias (matches).

## Tecnologías utilizadas 💻

- **Lenguaje:** C# (.NET 8.0)
- **Base de datos:** MySQL
- **Interfaz de consola:** Spectre.Console (para una experiencia visual mejorada)
- **Arquitectura:** Clean Architecture (Arquitectura Limpia)
- **Patrón de diseño principal:** Strategy Pattern

## Estructura del proyecto 📂

El proyecto sigue los principios de Clean Architecture y está organizado en las siguientes carpetas:

```
campusLove/
│
├── domain/                  # Capa de dominio
│   ├── entities/            # Entidades de negocio
│   ├── lists/               # Documentación y listas de referencia
│   ├── models/              # Modelos de datos
│   ├── ports/               # Interfaces/puertos
│   └── strategy/            # Implementación del patrón Strategy
│
├── application/             # Capa de aplicación
│   └── services/            # Servicios de la aplicación
│
├── infraestructure/         # Capa de infraestructura
│   └── mysql/               # Acceso a datos MySQL
│
├── database/                # Scripts de base de datos
│   ├── database.sql         # Estructura de la BD
│   └── inserts.sql          # Datos de ejemplo
│
├── diagrams/                # Diagramas del proyecto
│   ├── er_diagram.png       # Diagrama ER de la base de datos
│   └── class_diagram.png    # Diagrama de clases del software
│
└── Program.cs               # Punto de entrada de la aplicación
```

### Implementación del patrón Strategy 🧩

El patrón Strategy está implementado mediante:

- **IEmparejamientoStrategy:** Interface que define el contrato para encontrar perfiles compatibles
- **Implementaciones concretas:**
  - **EmparejamientoPorInteresesStrategy:** Empareja basado en intereses comunes
  - **EmparejamientoPorUbicacionStrategy:** Empareja basado en cercanía geográfica
  - **EmparejamientoPorEdadStrategy:** Empareja basado en rango de edad similar

El usuario puede cambiar entre estas estrategias desde el menú principal, y el sistema usará la estrategia seleccionada para mostrar perfiles.

## Instrucciones para ejecutar el proyecto ▶️

### Requisitos previos

- .NET 8.0 SDK
- MySQL Server
- Visual Studio o Visual Studio Code (opcional)

### Pasos para la configuración

1. **Clonar el repositorio:**
   ```bash
   git clone [URL del repositorio]
   cd campusLove
   ```

2. **Crear la base de datos:**
   - Ejecuta el script `database/database.sql` en tu servidor MySQL para crear la estructura
   - Ejecuta el script `database/inserts.sql` para cargar datos de ejemplo

3. **Configurar la conexión a la base de datos:**
   - Abre el archivo `Program.cs`
   - Modifica la cadena de conexión según tu configuración:
     ```csharp
     private static string connectionString = "Server=localhost;Database=campusLove;User=root;Password=TU_PASSWORD;";
     ```

4. **Compilar y ejecutar:**
   ```bash
   dotnet build
   dotnet run
   ```

5. **Usuarios de prueba:**
   - Correo: `maria@email.com` | Contraseña: `maria123`
   - Correo: `juan@email.com` | Contraseña: `juan123`
   - O puedes registrar un nuevo usuario desde el menú principal

### Modo multicliente

El sistema cuenta con un "Modo multicliente" que permite cambiar rápidamente entre diferentes usuarios para probar las funcionalidades. Se puede acceder desde la opción 5 del menú principal después de iniciar sesión.

## Funcionalidades principales 🚀

- **Registro e inicio de sesión**
- **Visualización de perfiles** según la estrategia de emparejamiento seleccionada
- **Sistema de likes/dislikes** con límite diario
- **Visualización de coincidencias** (matches cuando hay likes mutuos)
- **Estadísticas** de popularidad y actividad
- **Selección dinámica de estrategias** de emparejamiento



## Posibles mejoras futuras 🔮

- Implementar más estrategias de emparejamiento (por carrera, personalidad, etc.)
- Agregar un sistema de mensajería entre usuarios que coinciden
- Migrar a una interfaz gráfica (web o móvil)

## ¿Qué sigue? 🚀

Me gustaría implementar cuando tenga tiempo:

- Hacer una versión web con interface gráfica con buen diseño.
- Añadir más algoritmos de emparejamiento (¿por música favorita tal vez?)
- Integrar un sistema de mensajería para que los matches puedan hablar




