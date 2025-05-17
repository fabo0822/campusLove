CREATE DATABASE campusLove;

USE  campusLove;	


-- Tabla de géneros
CREATE TABLE generos (
	id INT PRIMARY KEY,
	descripcion VARCHAR(20) NOT NULL
);

-- Tabla de países
CREATE TABLE paises (
	id INT PRIMARY KEY,
	nombre VARCHAR(100) NOT NULL
);

-- Tabla de departamentos
CREATE TABLE departamentos (
	id INT PRIMARY KEY,
	nombre VARCHAR(100) NOT NULL,
	pais_id INT,
	FOREIGN KEY (pais_id) REFERENCES paises(id)
);

-- Tabla de ciudades
CREATE TABLE ciudades (
	id INT PRIMARY KEY,
	nombre VARCHAR(100) NOT NULL,
	departamento_id INT,
	FOREIGN KEY (departamento_id) REFERENCES departamentos(id)
);

-- Tabla de usuarios
CREATE TABLE usuarios (
	id INT AUTO_INCREMENT PRIMARY KEY,
	nombre VARCHAR(255),
	edad INT,
	genero INT,
	intereses TEXT,
	carrera VARCHAR(255),
	frase TEXT,
	likes_diarios INT,
	max_likes_diarios INT,
	ciudad_id INT,
	FOREIGN KEY (genero) REFERENCES generos(id),
	FOREIGN KEY (ciudad_id) REFERENCES ciudades(id)
);

-- Tabla de login
CREATE TABLE login (
	id INT AUTO_INCREMENT PRIMARY KEY,
	usuario_id INT,
	correo VARCHAR(255) UNIQUE NOT NULL,
	contrasena VARCHAR(255) NOT NULL,
	FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);

-- Tabla de interacciones
CREATE TABLE interacciones (
	id INT AUTO_INCREMENT PRIMARY KEY,
	usuario_id INT,
	objetivo_usuario_id INT,
	le_gusto BOOLEAN,
	fecha_interaccion DATETIME,
	FOREIGN KEY (usuario_id) REFERENCES usuarios(id),
	FOREIGN KEY (objetivo_usuario_id) REFERENCES usuarios(id)
);

-- Tabla de coincidencias
CREATE TABLE coincidencias (
	id INT AUTO_INCREMENT PRIMARY KEY,
	usuario1_id INT,
	usuario2_id INT,
	fecha_coincidencia DATETIME,
	FOREIGN KEY (usuario1_id) REFERENCES usuarios(id),
	FOREIGN KEY (usuario2_id) REFERENCES usuarios(id)
);

-- Tabla de estadísticas
CREATE TABLE estadisticas (
	id INT AUTO_INCREMENT PRIMARY KEY,
	usuario_id INT,
	likes_recibidos INT,
	coincidencias_totales INT,
	FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);