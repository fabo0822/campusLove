using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using campusLove.infraestructure.mysql;
using campusLove.application.services;
using campusLove.domain.ports;
using campusLove.domain.models;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Campus Love: Una aplicación de citas para estudiantes universitarios
/// Implementada con arquitectura limpia (Clean Architecture) y patrones de diseño
/// como Strategy para diferentes algoritmos de emparejamiento.
/// </summary>
namespace campusLove
{
    /// <summary>
    /// Clase principal que inicia la aplicación y configura las dependencias
    /// </summary>
    class Program
    {
        /// <summary>
        /// Cadena de conexión a la base de datos MySQL
        /// </summary>
        private static string connectionString = "Server=localhost;Database=campusLove;User=root;Password=1234;";
        
        /// <summary>
        /// Factory para crear conexiones a la base de datos
        /// </summary>
        private static readonly MySqlDbFactory _dbFactory;
        
        /// <summary>
        /// Servicio que gestiona usuarios y el patrón Strategy para emparejamiento
        /// </summary>
        private static readonly UsuarioService _usuarioService;
        
        /// <summary>
        /// Servicio que gestiona las interacciones entre usuarios (likes/dislikes)
        /// </summary>
        private static readonly InteraccionService _interaccionService;
        
        /// <summary>
        /// Servicio que gestiona las estadísticas de usuarios
        /// </summary>
        private static readonly IEstadisticaService _estadisticaService;
        
        /// <summary>
        /// Servicio que gestiona la autenticación de usuarios
        /// </summary>
        private static readonly AuthService _authService;
        
        /// <summary>
        /// Servicio que gestiona la interfaz de usuario en consola
        /// </summary>
        private static readonly UIService _uiService;
        
        /// <summary>
        /// Servicio de validación de datos
        /// </summary>
        private static readonly ValidacionService _validacionService;

        /// <summary>
        /// Constructor estático que inicializa todos los servicios necesarios para la aplicación
        /// </summary>
        /// <remarks>
        /// Este método implementa el principio de inyección de dependencias, pasando
        /// las dependencias necesarias a cada servicio. Esto facilita la modularidad
        /// y pruebas unitarias, además de permitir intercambiar implementaciones
        /// (como en el caso del patrón Strategy).
        /// </remarks>
        static Program()
        {
            try
            {
                // Inicializamos la factory de base de datos primero ya que es requerida por los demás servicios
                _dbFactory = new MySqlDbFactory(connectionString);
                
                // Inicializamos los servicios en orden de dependencia
                _validacionService = new ValidacionService();
                _usuarioService = new UsuarioService(_dbFactory); // Contiene las estrategias de emparejamiento
                _interaccionService = new InteraccionService(_dbFactory);
                _estadisticaService = new EstadisticaService(_dbFactory);
                _authService = new AuthService(_dbFactory);
                
                // El UIService depende de todos los demás servicios
                _uiService = new UIService(
                    _dbFactory, 
                    _usuarioService, 
                    _interaccionService, 
                    (EstadisticaService)_estadisticaService, 
                    _authService);

                Console.WriteLine("Servicios inicializados correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al inicializar los servicios: {ex.Message}");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Punto de entrada principal de la aplicación
        /// </summary>
        /// <param name="args">Argumentos de línea de comandos (no utilizados)</param>
        /// <remarks>
        /// El flujo principal de la aplicación es:
        /// 1. Mostrar el menú de login si no hay usuario autenticado
        /// 2. Mostrar el menú principal si hay un usuario autenticado
        /// 3. Repetir hasta que el usuario decida salir
        /// 
        /// El patrón Strategy se utiliza dentro del UsuarioService para
        /// permitir diferentes algoritmos de emparejamiento que pueden
        /// ser seleccionados desde el menú principal.
        /// </remarks>
        static async Task Main(string[] args)
        {
            try
            {
                bool running = true;
                while (running)
                {
                    // Si no hay usuario autenticado, mostrar el menú de login
                    if (_uiService.GetCurrentUserId() == -1)
                    {
                        await _uiService.MostrarMenuLogin();
                    }
                    else
                    {
                        // Si hay un usuario autenticado, mostrar el menú principal
                        await _uiService.MostrarMenuPrincipal();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en la aplicación: {ex.Message}");
                Console.WriteLine("Presione cualquier tecla para salir...");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }
    }

    /// <summary>
    /// Clase de utilidad que proporciona métodos para validar datos de entrada
    /// </summary>
    /// <remarks>
    /// Esta clase contiene métodos estáticos para validar diferentes tipos de datos,
    /// como correos electrónicos y contraseñas, asegurando que cumplan con los requisitos
    /// de formato y seguridad adecuados.
    /// </remarks>
    public static class Validaciones
    {
        /// <summary>
        /// Valida si un correo electrónico tiene un formato válido
        /// </summary>
        /// <param name="correo">El correo electrónico a validar</param>
        /// <returns>True si el correo es válido, false en caso contrario</returns>
        public static bool EsCorreoValido(string correo)
        {
            try {
                // Intenta crear un objeto MailAddress con el correo proporcionado
                // Si no lanza una excepción, el formato es válido
                var addr = new System.Net.Mail.MailAddress(correo);
                return addr.Address == correo;
            }
            catch {
                return false;
            }
        }

        /// <summary>
        /// Valida si una contraseña cumple con los requisitos mínimos de seguridad
        /// </summary>
        /// <param name="contrasena">La contraseña a validar</param>
        /// <returns>True si la contraseña cumple los requisitos, false en caso contrario</returns>
        /// <remarks>
        /// Requisitos:
        /// - Al menos 6 caracteres de longitud
        /// - Al menos una letra mayúscula
        /// - Al menos una letra minúscula
        /// - Al menos un dígito
        /// </remarks>
        public static bool EsContrasenaValida(string contrasena)
        {
            return contrasena.Length >= 6 &&
                   contrasena.Any(char.IsUpper) &&
                   contrasena.Any(char.IsLower) &&
                   contrasena.Any(char.IsDigit);
        }
    }
}
