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
            Console.Clear();
            Console.WriteLine("=== Iniciar Sesión ===");
            Console.Write("Correo: ");
            string correo = Console.ReadLine();
            Console.Write("Contraseña: ");
            string contrasena = Console.ReadLine();

            var (success, userId) = await _authService.Login(correo, contrasena);
            if (success)
            {
                currentUserId = userId;
                Console.WriteLine("Login exitoso. Presione cualquier tecla para continuar.");
            }
            else
            {
                Console.WriteLine("Credenciales inválidas. Presione cualquier tecla para continuar.");
            }
            Console.ReadKey();
        }

        static async Task RegistrarUsuario()
        {
            Console.Clear();
            Console.WriteLine("=== Registro de Usuario ===");
            Console.Write("Nombre: ");
            string nombre = Console.ReadLine();
            Console.Write("Edad: ");
            int edad = int.Parse(Console.ReadLine());
            Console.Write("Género (1-Masculino, 2-Femenino): ");
            int genero = int.Parse(Console.ReadLine());
            Console.Write("Intereses: ");
            string intereses = Console.ReadLine();
            Console.Write("Carrera: ");
            string carrera = Console.ReadLine();
            Console.Write("Frase: ");
            string frase = Console.ReadLine();
            Console.Write("Ciudad ID: ");
            int ciudadId = int.Parse(Console.ReadLine());
            Console.Write("Correo: ");
            string correo = Console.ReadLine();
            Console.Write("Contraseña: ");
            string contrasena = Console.ReadLine();

            await _usuarioService.RegistrarUsuario(nombre, edad, genero, intereses, carrera, frase, ciudadId);
            
            // Obtener el ID del usuario recién registrado
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT LAST_INSERT_ID()", (MySqlConnection)conn);
                int nuevoUsuarioId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                
                // Registrar las credenciales de login
                bool loginRegistrado = await _authService.RegistrarLogin(nuevoUsuarioId, correo, contrasena);
                if (loginRegistrado)
                {
                    Console.WriteLine("Usuario registrado exitosamente.");
                    currentUserId = nuevoUsuarioId;
                }
                else
                {
                    Console.WriteLine("Error al registrar las credenciales de login.");
                }
            }
            Console.WriteLine("Presione cualquier tecla para continuar.");
            Console.ReadKey();
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
                    @"SELECT c.id, u.nombre 
                    FROM coincidencias c 
                    JOIN usuarios u ON c.usuario2_id = u.id 
                    WHERE c.usuario1_id = @currentUserId", 
                    (MySqlConnection)conn);
                
                cmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Console.WriteLine($"Coincidencia ID: {reader["id"]}, Usuario: {reader["nombre"]}");
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
    }
}
