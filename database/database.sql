CREATE DATABASE campusLove;

USE  campusLove;	


CREATE TABLE generos (
	id INT PRIMARY KEY,
	descripcion VARCHAR(20) NOT NULL
);


CREATE TABLE paises (
	id INT PRIMARY KEY,
	nombre VARCHAR(100) NOT NULL
);


CREATE TABLE departamentos (
	id INT PRIMARY KEY,
	nombre VARCHAR(100) NOT NULL,
	pais_id INT,
	FOREIGN KEY (pais_id) REFERENCES paises(id)
);


CREATE TABLE ciudades (
	id INT PRIMARY KEY,
	nombre VARCHAR(100) NOT NULL,
	departamento_id INT,
	FOREIGN KEY (departamento_id) REFERENCES departamentos(id)
);


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


CREATE TABLE login (
	id INT AUTO_INCREMENT PRIMARY KEY,
	usuario_id INT,
	correo VARCHAR(255) UNIQUE NOT NULL,
	contrasena VARCHAR(255) NOT NULL,
	es_admin BOOLEAN NOT NULL DEFAULT FALSE,
	FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);


CREATE TABLE interacciones (
	id INT AUTO_INCREMENT PRIMARY KEY,
	usuario_id INT,             -- Usuario que envió el like/dislike
	objetivo_usuario_id INT,    -- Usuario que recibió el like/dislike
	le_gusto BOOLEAN,           -- true = like, false = dislike
	fecha_interaccion DATETIME, -- Momento en que ocurrió la interacción
	FOREIGN KEY (usuario_id) REFERENCES usuarios(id),
	FOREIGN KEY (objetivo_usuario_id) REFERENCES usuarios(id)
);


CREATE TABLE coincidencias (
	id INT AUTO_INCREMENT PRIMARY KEY,
	usuario1_id INT,             -- Primer usuario de la coincidencia
	usuario2_id INT,             -- Segundo usuario de la coincidencia
	fecha_coincidencia DATETIME, -- Momento en que se creó la coincidencia
	FOREIGN KEY (usuario1_id) REFERENCES usuarios(id),
	FOREIGN KEY (usuario2_id) REFERENCES usuarios(id)
);


CREATE TABLE estadisticas (
	id INT AUTO_INCREMENT PRIMARY KEY,
	usuario_id INT,             -- Usuario al que pertenecen las estadísticas
	likes_recibidos INT,        -- Número total de likes recibidos
	coincidencias_totales INT,  -- Número total de coincidencias (matches)
	FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);