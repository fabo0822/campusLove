using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using campusLove.infraestructure.mysql;
using campusLove.application.services;

namespace campusLove
{
    class Program
    {
        private static string connectionString = "Server=localhost;Database=campusLove;User=root;Password=1234;";
        private static int currentUserId = -1;
        private static readonly MySqlDbFactory _dbFactory;
        private static readonly UsuarioService _usuarioService;
        private static readonly InteraccionService _interaccionService;
        private static readonly EstadisticaService _estadisticaService;
        private static readonly AuthService _authService;

        static Program()
        {
            _dbFactory = new MySqlDbFactory(connectionString);
            _usuarioService = new UsuarioService(_dbFactory);
            _interaccionService = new InteraccionService(_dbFactory);
            _estadisticaService = new EstadisticaService(_dbFactory);
            _authService = new AuthService(_dbFactory);
        }

        static async Task Main(string[] args)
        {
            bool running = true;
            while (running)
            {
                if (currentUserId == -1)
                {
                    await MostrarMenuLogin();
                }
                else
                {
                    await MostrarMenuPrincipal();
                }
            }
        }

        static async Task MostrarMenuLogin()
        {
            Console.Clear();
            Console.WriteLine("=== Menú de Acceso ===");
            Console.WriteLine("1. Iniciar Sesión");
            Console.WriteLine("2. Registrarse");
            Console.WriteLine("3. Salir");
            Console.Write("Seleccione una opción: ");
            string opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    await Login();
                    break;
                case "2":
                    await RegistrarUsuario();
                    break;
                case "3":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Opción no válida. Presione cualquier tecla para continuar.");
                    Console.ReadKey();
                    break;
            }
        }

        static async Task MostrarMenuPrincipal()
        {
            Console.Clear();
            Console.WriteLine("=== Menú Principal ===");
            Console.WriteLine("1. Ver perfiles y dar Like o Dislike");
            Console.WriteLine("2. Ver mis coincidencias (matches)");
            Console.WriteLine("3. Ver estadísticas del sistema");
            Console.WriteLine("4. Cerrar Sesión");
            Console.WriteLine("5. Salir");
            Console.Write("Seleccione una opción: ");
            string opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    await VerPerfiles();
                    break;
                case "2":
                    await VerCoincidencias();
                    break;
                case "3":
                    await VerEstadisticas();
                    break;
                case "4":
                    currentUserId = -1;
                    break;
                case "5":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Opción no válida. Presione cualquier tecla para continuar.");
                    Console.ReadKey();
                    break;
            }
        }

        static async Task Login()
        {
            bool intentarDeNuevo = true;
            while (intentarDeNuevo)
            {
                Console.Clear();
                Console.WriteLine("=== Iniciar Sesión ===");
                Console.Write("Correo: ");
                string correo = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(correo))
                {
                    Console.WriteLine("El correo no puede estar vacío.");
                    Console.WriteLine("Presione ENTER para intentar de nuevo o ESC para volver al menú principal.");
                    if (Console.ReadKey().Key == ConsoleKey.Escape)
                        return;
                    continue;
                }

                Console.Write("Contraseña: ");
                string contrasena = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(contrasena))
                {
                    Console.WriteLine("La contraseña no puede estar vacía.");
                    Console.WriteLine("Presione ENTER para intentar de nuevo o ESC para volver al menú principal.");
                    if (Console.ReadKey().Key == ConsoleKey.Escape)
                        return;
                    continue;
                }

                try
                {
                    var (success, userId) = await _authService.Login(correo, contrasena);
                    if (success)
                    {
                        currentUserId = userId;
                        Console.WriteLine("Login exitoso. Presione cualquier tecla para continuar.");
                        Console.ReadKey();
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Credenciales inválidas.");
                        Console.WriteLine("Presione ENTER para intentar de nuevo o ESC para volver al menú principal.");
                        if (Console.ReadKey().Key == ConsoleKey.Escape)
                            return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al intentar iniciar sesión: {ex.Message}");
                    Console.WriteLine("Presione ENTER para intentar de nuevo o ESC para volver al menú principal.");
                    if (Console.ReadKey().Key == ConsoleKey.Escape)
                        return;
                }
            }
        }

        static async Task RegistrarUsuario()
        {
            bool intentarDeNuevo = true;
            while (intentarDeNuevo)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("=== Registro de Usuario ===");
                    
                    string nombre;
                    do {
                        Console.Write("Nombre (mínimo 3 caracteres): ");
                        nombre = Console.ReadLine()?.Trim();
                        if (string.IsNullOrWhiteSpace(nombre) || nombre.Length < 3)
                        {
                            Console.WriteLine("El nombre debe tener al menos 3 caracteres.");
                            Console.WriteLine("Presione ENTER para intentar de nuevo o ESC para volver al menú principal.");
                            if (Console.ReadKey().Key == ConsoleKey.Escape)
                                return;
                        }
                    } while (string.IsNullOrWhiteSpace(nombre) || nombre.Length < 3);

                    int edad;
                    bool edadValida;
                    do {
                        Console.Write("Edad (entre 18 y 99): ");
                        edadValida = int.TryParse(Console.ReadLine(), out edad) && edad >= 18 && edad <= 99;
                        if (!edadValida)
                        {
                            Console.WriteLine("La edad debe ser un número entre 18 y 99.");
                            Console.WriteLine("Presione ENTER para intentar de nuevo o ESC para volver al menú principal.");
                            if (Console.ReadKey().Key == ConsoleKey.Escape)
                                return;
                        }
                    } while (!edadValida);

                    int genero;
                    bool generoValido;
                    do {
                        Console.Write("Género (1-Masculino, 2-Femenino): ");
                        generoValido = int.TryParse(Console.ReadLine(), out genero) && (genero == 1 || genero == 2);
                        if (!generoValido)
                        {
                            Console.WriteLine("El género debe ser 1 (Masculino) o 2 (Femenino).");
                            Console.WriteLine("Presione ENTER para intentar de nuevo o ESC para volver al menú principal.");
                            if (Console.ReadKey().Key == ConsoleKey.Escape)
                                return;
                        }
                    } while (!generoValido);

                    string intereses;
                    do {
                        Console.Write("Intereses (mínimo 5 caracteres): ");
                        intereses = Console.ReadLine()?.Trim();
                        if (string.IsNullOrWhiteSpace(intereses) || intereses.Length < 5)
                        {
                            Console.WriteLine("Los intereses deben tener al menos 5 caracteres.");
                            Console.WriteLine("Presione ENTER para intentar de nuevo o ESC para volver al menú principal.");
                            if (Console.ReadKey().Key == ConsoleKey.Escape)
                                return;
                        }
                    } while (string.IsNullOrWhiteSpace(intereses) || intereses.Length < 5);

                    string carrera;
                    do {
                        Console.Write("Carrera (mínimo 3 caracteres): ");
                        carrera = Console.ReadLine()?.Trim();
                        if (string.IsNullOrWhiteSpace(carrera) || carrera.Length < 3)
                        {
                            Console.WriteLine("La carrera debe tener al menos 3 caracteres.");
                            Console.WriteLine("Presione ENTER para intentar de nuevo o ESC para volver al menú principal.");
                            if (Console.ReadKey().Key == ConsoleKey.Escape)
                                return;
                        }
                    } while (string.IsNullOrWhiteSpace(carrera) || carrera.Length < 3);

                    string frase;
                    do {
                        Console.Write("Frase (mínimo 5 caracteres): ");
                        frase = Console.ReadLine()?.Trim();
                        if (string.IsNullOrWhiteSpace(frase) || frase.Length < 5)
                        {
                            Console.WriteLine("La frase debe tener al menos 5 caracteres.");
                            Console.WriteLine("Presione ENTER para intentar de nuevo o ESC para volver al menú principal.");
                            if (Console.ReadKey().Key == ConsoleKey.Escape)
                                return;
                        }
                    } while (string.IsNullOrWhiteSpace(frase) || frase.Length < 5);

                    int ciudadId;
                    bool ciudadValida;
                    do {
                        await MostrarCiudadesDisponibles();
                        Console.Write("Seleccione el ID de la ciudad: ");
                        ciudadValida = int.TryParse(Console.ReadLine(), out ciudadId);
                        
                        if (ciudadValida)
                        {
                            // Verificar si la ciudad existe
                            using (var conn = _dbFactory.CreateConnection())
                            {
                                conn.Open();
                                var cmd = new MySqlCommand(
                                    "SELECT COUNT(*) FROM ciudades WHERE id = @ciudadId", 
                                    (MySqlConnection)conn);
                                cmd.Parameters.AddWithValue("@ciudadId", ciudadId);
                                ciudadValida = Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
                            }
                        }

                        if (!ciudadValida)
                        {
                            Console.WriteLine("Por favor, seleccione un ID de ciudad válido de la lista.");
                            Console.WriteLine("Presione ENTER para intentar de nuevo o ESC para volver al menú principal.");
                            if (Console.ReadKey().Key == ConsoleKey.Escape)
                                return;
                        }
                    } while (!ciudadValida);

                    string correo;
                    do {
                        Console.Write("Correo (debe contener @ y .): ");
                        correo = Console.ReadLine()?.Trim().ToLower();
                        if (string.IsNullOrWhiteSpace(correo) || !correo.Contains("@") || !correo.Contains("."))
                        {
                            Console.WriteLine("El correo debe contener @ y .");
                            Console.WriteLine("Presione ENTER para intentar de nuevo o ESC para volver al menú principal.");
                            if (Console.ReadKey().Key == ConsoleKey.Escape)
                                return;
                        }
                    } while (string.IsNullOrWhiteSpace(correo) || !correo.Contains("@") || !correo.Contains("."));

                    string contrasena;
                    do {
                        Console.Write("Contraseña (mínimo 6 caracteres): ");
                        contrasena = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < 6)
                        {
                            Console.WriteLine("La contraseña debe tener al menos 6 caracteres.");
                            Console.WriteLine("Presione ENTER para intentar de nuevo o ESC para volver al menú principal.");
                            if (Console.ReadKey().Key == ConsoleKey.Escape)
                                return;
                        }
                    } while (string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < 6);

                    await _usuarioService.RegistrarUsuario(nombre, edad, genero, intereses, carrera, frase, ciudadId);
                    
                    using (var conn = _dbFactory.CreateConnection())
                    {
                        conn.Open();
                        var cmd = new MySqlCommand("SELECT LAST_INSERT_ID()", (MySqlConnection)conn);
                        int nuevoUsuarioId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        
                        bool loginRegistrado = await _authService.RegistrarLogin(nuevoUsuarioId, correo, contrasena);
                        if (loginRegistrado)
                        {
                            Console.WriteLine("Usuario registrado exitosamente.");
                            currentUserId = nuevoUsuarioId;
                            Console.WriteLine("Presione cualquier tecla para continuar.");
                            Console.ReadKey();
                            return;
                        }
                        else
                        {
                            Console.WriteLine("Error al registrar las credenciales de login.");
                            Console.WriteLine("Presione ENTER para intentar de nuevo o ESC para volver al menú principal.");
                            if (Console.ReadKey().Key == ConsoleKey.Escape)
                                return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al registrar usuario: {ex.Message}");
                    Console.WriteLine("Presione ENTER para intentar de nuevo o ESC para volver al menú principal.");
                    if (Console.ReadKey().Key == ConsoleKey.Escape)
                        return;
                }
            }
        }

        static async Task VerPerfiles()
        {
            Console.Clear();
            Console.WriteLine("=== Ver Perfiles ===");
            await _usuarioService.MostrarPerfiles(currentUserId);

            Console.Write("Ingrese el ID del usuario para dar Like (1) o Dislike (0): ");
            int targetId = int.Parse(Console.ReadLine());
            Console.Write("¿Le gustó? (1-Sí, 0-No): ");
            bool leGusto = Console.ReadLine() == "1";

            await _interaccionService.RegistrarInteraccion(currentUserId, targetId, leGusto);
            if (leGusto)
            {
                await _interaccionService.VerificarCoincidencia(currentUserId, targetId);
            }
            await _estadisticaService.ActualizarEstadisticas(currentUserId);
            await _estadisticaService.ActualizarEstadisticas(targetId);

            Console.WriteLine("Interacción registrada.");
            Console.WriteLine("Presione cualquier tecla para continuar.");
            Console.ReadKey();
        }

        static async Task VerCoincidencias()
        {
            Console.Clear();
            Console.WriteLine("=== Mis Coincidencias ===");
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT c.id, 
                        CASE 
                            WHEN c.usuario1_id = @currentUserId THEN u2.nombre
                            ELSE u1.nombre
                        END as nombre_match
                    FROM coincidencias c 
                    JOIN usuarios u1 ON c.usuario1_id = u1.id 
                    JOIN usuarios u2 ON c.usuario2_id = u2.id 
                    WHERE c.usuario1_id = @currentUserId 
                    OR c.usuario2_id = @currentUserId", 
                    (MySqlConnection)conn);
                
                cmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    bool tieneCoincidencias = false;
                    while (await reader.ReadAsync())
                    {
                        tieneCoincidencias = true;
                        Console.WriteLine($"Coincidencia ID: {reader["id"]}, Usuario: {reader["nombre_match"]}");
                    }
                    
                    if (!tieneCoincidencias)
                    {
                        Console.WriteLine("Aún no tienes coincidencias. ¡Sigue dando likes para encontrar tu match!");
                    }
                }
            }
            Console.WriteLine("Presione cualquier tecla para continuar.");
            Console.ReadKey();
        }

        static async Task VerEstadisticas()
        {
            Console.Clear();
            Console.WriteLine("=== Estadísticas del Sistema ===");
            await _estadisticaService.MostrarEstadisticas();
            Console.WriteLine("Presione cualquier tecla para continuar.");
            Console.ReadKey();
        }

        static void ModoMulticliente()
        {
            Console.Clear();
            Console.WriteLine("=== Modo Multicliente Ficticio ===");
            Console.Write("Ingrese el ID del usuario a simular: ");
            currentUserId = int.Parse(Console.ReadLine());
            Console.WriteLine($"Simulando como usuario ID: {currentUserId}");
            Console.WriteLine("Presione cualquier tecla para continuar.");
            Console.ReadKey();
        }

        private static async Task MostrarCiudadesDisponibles()
        {
            Console.WriteLine("\nCiudades disponibles:");
            Console.WriteLine("--------------------");
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT c.id, c.nombre, d.nombre as departamento 
                    FROM ciudades c 
                    JOIN departamentos d ON c.departamento_id = d.id 
                    ORDER BY d.nombre, c.nombre", 
                    (MySqlConnection)conn);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    string departamentoActual = "";
                    while (await reader.ReadAsync())
                    {
                        string departamento = reader["departamento"].ToString();
                        if (departamento != departamentoActual)
                        {
                            Console.WriteLine($"\n{departamento}:");
                            departamentoActual = departamento;
                        }
                        Console.WriteLine($"  [{reader["id"]}] {reader["nombre"]}");
                    }
                }
            }
            Console.WriteLine("--------------------\n");
        }
    }
}
