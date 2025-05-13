CREATE DATABASE campusLove,

USE DATABASE campusLove,	


-- Tabla de géneros
CREATE TABLE generos (
	id INT PRIMARY KEY,
	descripcion VARCHAR(20) NOT NULL
);

-- Tabla de usuarios (con la columna genero)
CREATE TABLE usuarios (
	id INT PRIMARY KEY,
	nombre VARCHAR(255),
	edad INT,
	genero_id INT,  
	intereses TEXT,
	carrera VARCHAR(255),
	frase TEXT,
	likes_diarios INT,
	max_likes_diarios INT,
	FOREIGN KEY (genero_id) REFERENCES genero(id)
);

-- Tabla de interacciones
CREATE TABLE interacciones (
	id INT PRIMARY KEY,
	usuario_id INT,
	objetivo_usuario_id INT,
	le_gusto BOOLEAN,
	fecha_interaccion DATETIME,
	FOREIGN KEY (usuario_id) REFERENCES usuarios(id),
	FOREIGN KEY (objetivo_usuario_id) REFERENCES usuarios(id)
);

-- Tabla de coincidencias
CREATE TABLE coincidencias (
	id INT PRIMARY KEY,
	usuario1_id INT,
	usuario2_id INT,
	fecha_coincidencia DATETIME,
	FOREIGN KEY (usuario1_id) REFERENCES usuarios(id),
	FOREIGN KEY (usuario2_id) REFERENCES usuarios(id)
);

-- Tabla de estadísticas
CREATE TABLE estadisticas (
	id INT PRIMARY KEY,
	usuario_id INT,
	likes_recibidos INT,
	coincidencias_totales INT,
	FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);