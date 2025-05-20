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
(3, 'Valle del Cauca', 1),
(4, 'Santander', 1),
(5, 'Atlántico', 1),
(6, 'Boyacá', 1);

-- Inserts de ciudades
INSERT INTO ciudades (id, nombre, departamento_id) VALUES
(1, 'Medellín', 1),
(2, 'Bello', 1),
(3, 'Envigado', 1),
(4, 'Sabaneta', 1),
(5, 'Itagüí', 1),
(6, 'Bogotá', 2),
(7, 'Soacha', 2),
(8, 'Facatativá', 2),
(9, 'Zipaquirá', 2),
(10, 'Chía', 2),
(11, 'Cali', 3),
(12, 'Palmira', 3),
(13, 'Buenaventura', 3),
(14, 'Tuluá', 3),
(15, 'Bucaramanga', 4),
(16, 'Floridablanca', 4),
(17, 'Barrancabermeja', 4),
(18, 'Barranquilla', 5),
(19, 'Soledad', 5),
(20, 'Tunja', 6),
(21, 'Duitama', 6),
(22, 'Sogamoso', 6);

-- Inserts de usuarios
INSERT INTO usuarios (id, nombre, edad, genero, intereses, carrera, frase, likes_diarios, max_likes_diarios, ciudad_id) VALUES
(1, 'Maria', 22, 2, 'Videojuegos, Música', 'Ingeniería Informática', 'Buscando bugs en corazones 💻❤️', 5, 10, 1),
(2, 'Juan', 24, 1, 'Arte, Cine', 'Diseño Gráfico', 'Un match y te diseño el futuro 🎨', 3, 5, 6),
(3, 'Laura', 21, 2, 'Lectura, Café', 'Psicología', 'Te escucho con el corazón ☕📚', 8, 15, 8),
(4, 'Carlos', 23, 1, 'Deporte, Series', 'Medicina', 'El mejor remedio: una buena cita 🩺🍿', 8, 15, 2),
(5, 'Andrea', 22, 2, 'Debates, Viajes', 'Derecho', 'Argumenta tu amor 💼✈️', 8, 15, 11),
(6, 'Luis', 25, 1, 'Finanzas, Ajedrez', 'Administración', 'Invertir en amor, la mejor decisión 💰♟️', 8, 15, 12),
(7, 'Valentina', 20, 2, 'Yoga, Cocina', 'Nutrición', 'Alimenta tu alma y tu corazón 🧘‍♀️🍳', 6, 12, 3),
(8, 'Daniel', 25, 1, 'Música, Fotografía', 'Comunicación', 'Capturando momentos y corazones 🎧📷', 4, 10, 9),
(9, 'Natalia', 23, 2, 'Astronomía, Senderismo', 'Física', 'Explorando estrellas y nuevos caminos 🔭🏖️', 7, 12, 15),
(10, 'Javier', 26, 1, 'Tecnología, Gaming', 'Ingeniería Electrónica', 'Conectando circuitos y emociones 💻🕹️', 9, 15, 16),
(11, 'Camila', 22, 2, 'Baile, Música', 'Artes Escénicas', 'Bailemos al ritmo del amor 💃🎶', 7, 12, 18),
(12, 'Sebastián', 24, 1, 'Fútbol, Política', 'Ciencias Políticas', 'Debatiendo ideas, conquistando sonrisas ⚽🎙️', 5, 10, 10),
(13, 'Carolina', 21, 2, 'Pintura, Poesía', 'Literatura', 'Pintando versos en tu corazón 🎨📝', 8, 15, 7),
(14, 'Miguel', 27, 1, 'Cocina, Vinos', 'Gastronomía', 'La receta perfecta incluye dos corazones 🧑‍🍳🍷', 7, 12, 13),
(15, 'Isabella', 23, 2, 'Idiomas, Viajes', 'Relaciones Internacionales', 'Hablemos el lenguaje universal del amor 🌎😍', 6, 12, 5),
(16, 'Eduardo', 25, 1, 'Ciclismo, Natación', 'Educación Física', 'La vida es como una bicicleta, hay que pedalear para avanzar 🚴‍♂️🏊‍♂️', 5, 10, 14),
(17, 'Gabriela', 22, 2, 'Animales, Naturaleza', 'Veterinaria', 'Curando corazones de todas las especies 🐾💚', 8, 15, 20),
(18, 'Alejandro', 26, 1, 'Historia, Arqueología', 'Historia', 'Descubriendo el pasado, construyendo nuestro futuro 🏛️⏳', 7, 12, 4),
(19, 'Daniela', 24, 2, 'Moda, Diseño', 'Diseño de Modas', 'Diseñando la tela de nuestros sueños 👗💬', 9, 15, 19),
(20, 'Roberto', 27, 1, 'Economía, Política', 'Economía', 'Invirtiendo en un futuro juntos 💵📈', 6, 12, 17);

-- Inserts de login
INSERT INTO login (id, usuario_id, correo, contrasena, es_admin) VALUES
(1, 1, 'maria@email.com', 'maria123', TRUE),  -- María como administradora
(2, 2, 'juan@email.com', 'juan123', FALSE),
(3, 3, 'laura@email.com', 'laura123', FALSE),
(4, 4, 'carlos@email.com', 'carlos123', FALSE),
(5, 5, 'andrea@email.com', 'andrea123', FALSE),
(6, 6, 'luis@email.com', 'luis123', FALSE),
(7, 7, 'valentina@email.com', 'valentina123', FALSE),
(8, 8, 'daniel@email.com', 'daniel123', FALSE),
(9, 9, 'natalia@email.com', 'natalia123', FALSE),
(10, 10, 'javier@email.com', 'javier123', FALSE),
(11, 11, 'camila@email.com', 'camila123', FALSE),
(12, 12, 'sebastian@email.com', 'sebastian123', FALSE),
(13, 13, 'carolina@email.com', 'carolina123', FALSE),
(14, 14, 'miguel@email.com', 'miguel123', FALSE),
(15, 15, 'isabella@email.com', 'isabella123', FALSE),
(16, 16, 'eduardo@email.com', 'eduardo123', FALSE),
(17, 17, 'gabriela@email.com', 'gabriela123', FALSE),
(18, 18, 'alejandro@email.com', 'alejandro123', FALSE),
(19, 19, 'daniela@email.com', 'daniela123', FALSE),
(20, 20, 'roberto@email.com', 'roberto123', FALSE);

-- Inserts de interacciones
INSERT INTO interacciones (id, usuario_id, objetivo_usuario_id, le_gusto, fecha_interaccion) VALUES
(1, 1, 2, TRUE, '2024-07-27 10:00:00'),
(2, 2, 1, TRUE, '2024-07-27 10:00:00'),
(3, 1, 3, TRUE, '2024-07-27 11:00:00'),
(4, 3, 1, TRUE, '2024-07-27 11:30:00'),
(5, 4, 7, TRUE, '2024-07-27 12:00:00'),
(6, 7, 4, TRUE, '2024-07-27 12:15:00'),
(7, 5, 8, TRUE, '2024-07-27 13:00:00'),
(8, 8, 5, TRUE, '2024-07-27 13:30:00'),
(9, 6, 9, TRUE, '2024-07-27 14:00:00'),
(10, 9, 6, TRUE, '2024-07-27 14:30:00'),
(11, 10, 11, TRUE, '2024-07-27 15:00:00'),
(12, 11, 10, TRUE, '2024-07-27 15:30:00'),
(13, 12, 13, TRUE, '2024-07-27 16:00:00'),
(14, 13, 12, TRUE, '2024-07-27 16:30:00'),
(15, 14, 15, TRUE, '2024-07-27 17:00:00'),
(16, 15, 14, TRUE, '2024-07-27 17:30:00'),
(17, 16, 19, TRUE, '2024-07-27 18:00:00'),
(18, 19, 16, TRUE, '2024-07-27 18:30:00'),
(19, 17, 20, FALSE, '2024-07-27 19:00:00'),
(20, 18, 20, TRUE, '2024-07-27 19:30:00'),
(21, 20, 18, TRUE, '2024-07-27 20:00:00'),
(22, 1, 7, TRUE, '2024-07-28 10:00:00'),
(23, 7, 1, TRUE, '2024-07-28 10:30:00'),
(24, 3, 12, TRUE, '2024-07-28 11:00:00'),
(25, 12, 3, TRUE, '2024-07-28 11:30:00'),
(26, 5, 15, TRUE, '2024-07-28 12:00:00'),
(27, 15, 5, TRUE, '2024-07-28 12:30:00'),
(28, 2, 8, FALSE, '2024-07-28 13:00:00'),
(29, 4, 10, FALSE, '2024-07-28 13:30:00'),
(30, 6, 14, FALSE, '2024-07-28 14:00:00'),
(31, 9, 13, FALSE, '2024-07-28 14:30:00'),
(32, 11, 17, FALSE, '2024-07-28 15:00:00'),
(33, 16, 1, FALSE, '2024-07-28 15:30:00'),
(34, 18, 3, FALSE, '2024-07-28 16:00:00'),
(35, 19, 5, FALSE, '2024-07-28 16:30:00'),
(36, 20, 2, FALSE, '2024-07-28 17:00:00'),
(37, 13, 19, TRUE, '2024-07-28 17:30:00'),
(38, 19, 13, TRUE, '2024-07-28 18:00:00'),
(39, 8, 14, TRUE, '2024-07-28 18:30:00'),
(40, 14, 8, TRUE, '2024-07-28 19:00:00');

-- Inserts de coincidencias
INSERT INTO coincidencias (id, usuario1_id, usuario2_id, fecha_coincidencia) VALUES
(1, 1, 2, '2024-07-27 10:00:00'),
(2, 1, 3, '2024-07-27 11:30:00'),
(3, 4, 7, '2024-07-27 12:15:00'),
(4, 5, 8, '2024-07-27 13:30:00'),
(5, 6, 9, '2024-07-27 14:30:00'),
(6, 10, 11, '2024-07-27 15:30:00'),
(7, 12, 13, '2024-07-27 16:30:00'),
(8, 14, 15, '2024-07-27 17:30:00'),
(9, 16, 19, '2024-07-27 18:30:00'),
(10, 18, 20, '2024-07-27 20:00:00'),
(11, 1, 7, '2024-07-28 10:30:00'),
(12, 3, 12, '2024-07-28 11:30:00'),
(13, 5, 15, '2024-07-28 12:30:00'),
(14, 13, 19, '2024-07-28 18:00:00'),
(15, 8, 14, '2024-07-28 19:00:00');

