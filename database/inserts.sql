-- Inserts de géneros
INSERT INTO generos (id, descripcion) VALUES
(1, 'Masculino'),
(2, 'Femenino');

-- Inserts de países
INSERT INTO paises (id, nombre) VALUES
(1, 'Colombia');

-- Inserts de departamentos
INSERT INTO departamentos (id, nombre, pais_id) VALUES
(1, 'Antioquia', 1),
(2, 'Cundinamarca', 1),
(3, 'Valle del Cauca', 1);

-- Inserts de ciudades
INSERT INTO ciudades (id, nombre, departamento_id) VALUES
(1, 'Medellín', 1),
(2, 'Bello', 1),
(3, 'Envigado', 1),
(4, 'Bogotá', 2),
(5, 'Soacha', 2),
(6, 'Facatativá', 2),
(7, 'Cali', 3),
(8, 'Palmira', 3),
(9, 'Buenaventura', 3);

-- Inserts de usuarios
INSERT INTO usuarios (id, nombre, edad, genero, intereses, carrera, frase, likes_diarios, max_likes_diarios, ciudad_id) VALUES
(1, 'Maria', 22, 2, 'Videojuegos, Música', 'Ingeniería Informática', 'Buscando bugs en corazones 💻❤️', 5, 10, 1),
(2, 'Juan', 24, 1, 'Arte, Cine', 'Diseño Gráfico', 'Un match y te diseño el futuro 🎨', 3, 5, 4),
(3, 'Laura', 21, 2, 'Lectura, Café', 'Psicología', 'Te escucho con el corazón ☕📚', 8, 15, 6),
(4, 'Carlos', 23, 1, 'Deporte, Series', 'Medicina', 'El mejor remedio: una buena cita 🩺🍿', 8, 15, 2),
(5, 'Andrea', 22, 2, 'Debates, Viajes', 'Derecho', 'Argumenta tu amor 💼✈️', 8, 15, 7),
(6, 'Luis', 25, 1, 'Finanzas, Ajedrez', 'Administración', 'Invertir en amor, la mejor decisión 💰♟️', 8, 15, 8);

-- Inserts de login
INSERT INTO login (id, usuario_id, correo, contrasena) VALUES
(1, 1, 'maria@email.com', 'maria123'),
(2, 2, 'juan@email.com', 'juan123'),
(3, 3, 'laura@email.com', 'laura123'),
(4, 4, 'carlos@email.com', 'carlos123'),
(5, 5, 'andrea@email.com', 'andrea123'),
(6, 6, 'luis@email.com', 'luis123');

-- Inserts de interacciones
INSERT INTO interacciones (id, usuario_id, objetivo_usuario_id, le_gusto, fecha_interaccion) VALUES
(1, 1, 2, TRUE, '2024-07-27 10:00:00'),
(2, 2, 1, TRUE, '2024-07-27 10:00:00'),
(3, 1, 3, TRUE, '2024-07-27 11:00:00');

-- Inserts de coincidencias
INSERT INTO coincidencias (id, usuario1_id, usuario2_id, fecha_coincidencia) VALUES
(1, 1, 2, '2024-07-27 10:00:00');

-- Inserts de estadísticas
INSERT INTO estadisticas (id, usuario_id, likes_recibidos, coincidencias_totales) VALUES
(1, 1, 2, 1),
(2, 2, 1, 1),
(3, 3, 1, 0);