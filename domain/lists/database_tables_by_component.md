# Tablas de Base de Datos por Componente

Este documento muestra la relación entre los componentes de la aplicación CampusLove y las tablas de base de datos que utiliza cada uno.

## Componentes de Servicio

### UsuarioService
- **Tablas**: 
  - `usuarios` - Gestión de perfiles de usuario
  - `interacciones` - Para verificar interacciones previas al obtener perfiles
  - `ciudades` - Para información de ubicación en perfiles

### InteraccionService
- **Tablas**: 
  - `interacciones` - Registro de likes/dislikes entre usuarios
  - `usuarios` - Actualización de contadores de likes diarios
  - `coincidencias` - Creación y verificación de matches
  - `estadisticas` - Actualización de estadísticas cuando hay coincidencias

### AuthService
- **Tablas**: 
  - `login` - Autenticación de usuarios y registro de credenciales
  - `usuarios` - Relación con el usuario autenticado

### EstadisticaService
- **Tablas**: 
  - `estadisticas` - Gestión de métricas de usuario
  - `usuarios` - Relación con el usuario de las estadísticas
  - `interacciones` - Para calcular likes recibidos
  - `coincidencias` - Para calcular coincidencias totales

### AdminService
- **Tablas**: 
  - `usuarios` - Administración de usuarios
  - `login` - Gestión de credenciales
  - `generos` - Administración de catálogo de géneros
  - `paises` - Administración de catálogo de países
  - `departamentos` - Administración de catálogo de departamentos
  - `ciudades` - Administración de catálogo de ciudades
  - `interacciones` - Visualización de interacciones entre usuarios
  - `coincidencias` - Visualización de matches
  - `estadisticas` - Visualización de estadísticas

## Entidades (Domain)

- **Usuario**: Se relaciona con `usuarios`
- **Login**: Se relaciona con `login`
- **Genero**: Se relaciona con `generos`
- **Ciudad**: Se relaciona con `ciudades`
- **Departamento**: Se relaciona con `departamentos`
- **Pais**: Se relaciona con `paises`
- **Interaccion**: Se relaciona con `interacciones`
- **Coincidencia**: Se relaciona con `coincidencias`
- **Estadistica**: Se relaciona con `estadisticas`

## Resumen de Tablas

| Tabla | Componentes que la utilizan |
|-------|----------------------------|
| usuarios | UsuarioService, InteraccionService, AuthService, EstadisticaService, AdminService |
| login | AuthService, AdminService |
| generos | AdminService |
| paises | AdminService |
| departamentos | AdminService |
| ciudades | UsuarioService, AdminService |
| interacciones | UsuarioService, InteraccionService, EstadisticaService, AdminService |
| coincidencias | InteraccionService, EstadisticaService, AdminService |
| estadisticas | InteraccionService, EstadisticaService, AdminService |
