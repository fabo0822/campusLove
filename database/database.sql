-- ************************************************************
-- Campus Love: Sistema de Citas para Estudiantes Universitarios
-- ************************************************************
-- Script de creación de la base de datos y tablas
-- Incluye todas las relaciones necesarias para la aplicación
-- Implementa estructura para soportar el patrón Strategy de emparejamiento

CREATE DATABASE campusLove;

USE  campusLove;	

-- ===================================
-- TABLAS PARA DATOS DE CATALOGOS
-- ===================================

-- Tabla de géneros: Almacena los tipos de género disponibles
-- Utilizada para perfiles de usuario
CREATE TABLE generos (
	id INT PRIMARY KEY,
	descripcion VARCHAR(20) NOT NULL
);

-- Tabla de países: Almacena la lista de países disponibles
-- Utilizada para perfiles de usuario y ubicaciones
CREATE TABLE paises (
	id INT PRIMARY KEY,
	nombre VARCHAR(100) NOT NULL
);

-- Tabla de departamentos: Divisiones administrativas de los países
-- Utilizada por la estrategia de emparejamiento por ubicación
CREATE TABLE departamentos (
	id INT PRIMARY KEY,
	nombre VARCHAR(100) NOT NULL,
	pais_id INT,
	FOREIGN KEY (pais_id) REFERENCES paises(id)
);

-- Tabla de ciudades: Localidades dentro de los departamentos
-- Crucial para la estrategia de emparejamiento por ubicación
CREATE TABLE ciudades (
	id INT PRIMARY KEY,
	nombre VARCHAR(100) NOT NULL,
	departamento_id INT,
	FOREIGN KEY (departamento_id) REFERENCES departamentos(id)
);

-- ===================================
-- TABLAS PRINCIPALES DE LA APLICACIÓN
-- ===================================

-- Tabla de usuarios: Almacena la información principal de los perfiles
-- Contiene los campos utilizados por las diferentes estrategias de emparejamiento:
-- - intereses: usado por EmparejamientoPorInteresesStrategy
-- - edad: usado por EmparejamientoPorEdadStrategy
-- - ciudad_id: usado por EmparejamientoPorUbicacionStrategy
-- También gestiona el límite de likes diarios configurable por usuario
CREATE TABLE usuarios (
	id INT AUTO_INCREMENT PRIMARY KEY,
	nombre VARCHAR(255),
	edad INT,
	genero INT,
	intereses TEXT,
	carrera VARCHAR(255),
	frase TEXT,
	likes_diarios INT,        -- Contador de likes dados en el día actual
	max_likes_diarios INT,    -- Límite máximo de likes que puede dar por día
	ciudad_id INT,
	FOREIGN KEY (genero) REFERENCES generos(id),
	FOREIGN KEY (ciudad_id) REFERENCES ciudades(id)
);

-- Tabla de login: Almacena las credenciales de acceso de los usuarios
-- Separada de la tabla de usuarios para mejorar la seguridad y modularidad
CREATE TABLE login (
	id INT AUTO_INCREMENT PRIMARY KEY,
	usuario_id INT,
	correo VARCHAR(255) UNIQUE NOT NULL,
	contrasena VARCHAR(255) NOT NULL,
	FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);

-- Tabla de interacciones: Registra los likes y dislikes entre usuarios
-- El campo le_gusto indica si fue un like (true) o dislike (false)
-- Se utiliza para generar coincidencias cuando hay likes mutuos
-- También para filtrar perfiles ya vistos en las estrategias de emparejamiento
CREATE TABLE interacciones (
	id INT AUTO_INCREMENT PRIMARY KEY,
	usuario_id INT,             -- Usuario que envió el like/dislike
	objetivo_usuario_id INT,    -- Usuario que recibió el like/dislike
	le_gusto BOOLEAN,           -- true = like, false = dislike
	fecha_interaccion DATETIME, -- Momento en que ocurrió la interacción
	FOREIGN KEY (usuario_id) REFERENCES usuarios(id),
	FOREIGN KEY (objetivo_usuario_id) REFERENCES usuarios(id)
);

-- Tabla de coincidencias: Registra los matches entre usuarios
-- Una coincidencia se crea cuando dos usuarios se dan like mutuamente
-- Esto ocurre automáticamente mediante el InteraccionService
CREATE TABLE coincidencias (
	id INT AUTO_INCREMENT PRIMARY KEY,
	usuario1_id INT,             -- Primer usuario de la coincidencia
	usuario2_id INT,             -- Segundo usuario de la coincidencia
	fecha_coincidencia DATETIME, -- Momento en que se creó la coincidencia
	FOREIGN KEY (usuario1_id) REFERENCES usuarios(id),
	FOREIGN KEY (usuario2_id) REFERENCES usuarios(id)
);

-- Tabla de estadísticas: Almacena métricas sobre la actividad de los usuarios
-- Se actualiza automáticamente mediante el EstadisticaService cuando hay interacciones
-- Utilizada para mostrar rankings y datos en la interfaz de usuario
CREATE TABLE estadisticas (
	id INT AUTO_INCREMENT PRIMARY KEY,
	usuario_id INT,             -- Usuario al que pertenecen las estadísticas
	likes_recibidos INT,        -- Número total de likes recibidos
	coincidencias_totales INT,  -- Número total de coincidencias (matches)
	FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);