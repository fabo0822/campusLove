-- Insertar usuarios (ahora con el campo genero)
INSERT INTO usuarios (id, nombre, edad, genero, intereses, carrera, frase, likes_diarios, max_likes_diarios) VALUES
(1, 'Maria', 22, 2, 'Ingeniería Informática', 'Videojuegos, Música', 'Buscando bugs en corazones 💻❤️', 5, 10),
(2, 'Juan', 24, 1, 'Diseño Gráfico', 'Arte, Cine', 'Un match y te diseño el futuro 🎨', 3, 5),
(3, 'Laura', 21, 2, 'Psicología', 'Lectura, Café', 'Te escucho con el corazón ☕📚', 8, 15),
(4, 'Carlos', 23, 1, 'Medicina', 'Deporte, Series', 'El mejor remedio: una buena cita 🩺🍿', 8, 15),
(5, 'Andrea', 22, 2, 'Derecho', 'Debates, Viajes', 'Argumenta tu amor 💼✈️', 8, 15),
(6, 'Luis', 25, 1, 'Administración', 'Finanzas, Ajedrez', 'Invertir en amor, la mejor decisión 💰♟️', 8, 15);

-- Insertar géneros
INSERT INTO generos (id, descripcion) VALUES
(1, 'Masculino'),
(2, 'Femenino');

-- Insertar interacciones
INSERT INTO interacciones (id, usuario_id, objetivo_usuario_id, le_gusto, fecha_interaccion) VALUES
(1, 1, 2, TRUE, '2024-07-27 10:00:00'),
(2, 2, 1, TRUE, '2024-07-27 10:00:00'),
(3, 1, 3, TRUE, '2024-07-27 11:00:00');

-- Insertar coincidencias
INSERT INTO coincidencias (id, usuario1_id, usuario2_id, fecha_coincidencia) VALUES
(1, 1, 2, '2024-07-27 10:00:00');