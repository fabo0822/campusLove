# Clases por Componente

Este documento muestra las clases que utiliza cada componente en la aplicación CampusLove. Una clase puede ser utilizada por múltiples componentes.

## Componentes de Servicio

### UsuarioService
- **Clases utilizadas**:
  - `MySqlDbFactory` - Para crear conexiones a la base de datos
  - `IEmparejamientoStrategy` - Interfaz para estrategias de emparejamiento
  - `EmparejamientoPorInteresesStrategy` - Implementación de estrategia por intereses
  - `EmparejamientoPorUbicacionStrategy` - Implementación de estrategia por ubicación
  - `EmparejamientoPorEdadStrategy` - Implementación de estrategia por edad
  - `PerfilUsuario` - Modelo para mostrar perfiles de usuarios
  - `Usuario` - Entidad de dominio para usuarios

### InteraccionService
- **Clases utilizadas**:
  - `MySqlDbFactory` - Para crear conexiones a la base de datos
  - `Interaccion` - Entidad de dominio para interacciones
  - `Coincidencia` - Entidad de dominio para coincidencias (matches)
  - `Usuario` - Entidad de dominio para usuarios
  - `Estadistica` - Entidad de dominio para estadísticas

### AuthService
- **Clases utilizadas**:
  - `MySqlDbFactory` - Para crear conexiones a la base de datos
  - `Login` - Entidad de dominio para credenciales
  - `Usuario` - Entidad de dominio para usuarios

### EstadisticaService
- **Clases utilizadas**:
  - `MySqlDbFactory` - Para crear conexiones a la base de datos
  - `IEstadisticaService` - Puerto primario para el servicio de estadísticas
  - `EstadisticaUsuario` - Modelo para estadísticas de usuario
  - `Usuario` - Entidad de dominio para usuarios
  - `Interaccion` - Entidad de dominio para interacciones
  - `Coincidencia` - Entidad de dominio para coincidencias

### AdminService
- **Clases utilizadas**:
  - `MySqlDbFactory` - Para crear conexiones a la base de datos
  - `Usuario` - Entidad de dominio para usuarios
  - `Login` - Entidad de dominio para credenciales
  - `Genero` - Entidad de dominio para géneros
  - `Pais` - Entidad de dominio para países
  - `Departamento` - Entidad de dominio para departamentos
  - `Ciudad` - Entidad de dominio para ciudades
  - `Interaccion` - Entidad de dominio para interacciones
  - `Coincidencia` - Entidad de dominio para coincidencias
  - `Estadistica` - Entidad de dominio para estadísticas
  - `AdminUIHelper` - Clase auxiliar para UI de administración

## Estrategias de Emparejamiento

### EmparejamientoPorInteresesStrategy
- **Clases utilizadas**:
  - `MySqlDbFactory` - Para crear conexiones a la base de datos
  - `PerfilUsuario` - Modelo para mostrar perfiles de usuarios
  - `IEmparejamientoStrategy` - Interfaz que implementa

### EmparejamientoPorUbicacionStrategy
- **Clases utilizadas**:
  - `MySqlDbFactory` - Para crear conexiones a la base de datos
  - `PerfilUsuario` - Modelo para mostrar perfiles de usuarios
  - `IEmparejamientoStrategy` - Interfaz que implementa

### EmparejamientoPorEdadStrategy
- **Clases utilizadas**:
  - `MySqlDbFactory` - Para crear conexiones a la base de datos
  - `PerfilUsuario` - Modelo para mostrar perfiles de usuarios
  - `IEmparejamientoStrategy` - Interfaz que implementa

## Repositorios

### UsuarioRepository
- **Clases utilizadas**:
  - `MySqlDbFactory` - Para crear conexiones a la base de datos
  - `IUsuarioRepository` - Puerto secundario que implementa
  - `Usuario` - Entidad de dominio para usuarios

### LoginRepository
- **Clases utilizadas**:
  - `MySqlDbFactory` - Para crear conexiones a la base de datos
  - `ILoginRepository` - Puerto secundario que implementa
  - `Login` - Entidad de dominio para credenciales
  - `Usuario` - Entidad de dominio para usuarios

### CiudadRepository
- **Clases utilizadas**:
  - `MySqlDbFactory` - Para crear conexiones a la base de datos
  - `ICiudadRepository` - Puerto secundario que implementa
  - `Ciudad` - Entidad de dominio para ciudades

### DepartamentoRepository
- **Clases utilizadas**:
  - `MySqlDbFactory` - Para crear conexiones a la base de datos
  - `IDepartamentoRepository` - Puerto secundario que implementa
  - `Departamento` - Entidad de dominio para departamentos

## Resumen de Clases Más Utilizadas

| Clase | Componentes que la utilizan |
|-------|----------------------------|
| MySqlDbFactory | UsuarioService, InteraccionService, AuthService, EstadisticaService, AdminService, Todos los repositorios, Todas las estrategias |
| Usuario | UsuarioService, InteraccionService, AuthService, EstadisticaService, AdminService, UsuarioRepository |
| PerfilUsuario | UsuarioService, Todas las estrategias de emparejamiento |
| Interaccion | InteraccionService, EstadisticaService, AdminService |
| Coincidencia | InteraccionService, EstadisticaService, AdminService |
| Estadistica | InteraccionService, EstadisticaService, AdminService |
| Login | AuthService, AdminService, LoginRepository |
