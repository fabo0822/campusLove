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

namespace campusLove
{
    class Program
    {
        private static string connectionString = "Server=localhost;Database=campusLove;User=root;Password=1234;";
        private static readonly MySqlDbFactory _dbFactory;
        private static readonly UsuarioService _usuarioService;
        private static readonly InteraccionService _interaccionService;
        private static readonly IEstadisticaService _estadisticaService;
        private static readonly AuthService _authService;
        private static readonly UIService _uiService;
        private static readonly ValidacionService _validacionService;

        static Program()
        {
            try
            {
                _dbFactory = new MySqlDbFactory(connectionString);
                _validacionService = new ValidacionService();
                _usuarioService = new UsuarioService(_dbFactory);
                _interaccionService = new InteraccionService(_dbFactory);
                _estadisticaService = new EstadisticaService(_dbFactory);
                _authService = new AuthService(_dbFactory);
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

        static async Task Main(string[] args)
        {
            try
            {
                bool running = true;
                while (running)
                {
                    if (_uiService.GetCurrentUserId() == -1)
                    {
                        await _uiService.MostrarMenuLogin();
                    }
                    else
                    {
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

    public static class Validaciones
    {
        public static bool EsCorreoValido(string correo)
        {
            try {
                var addr = new System.Net.Mail.MailAddress(correo);
                return addr.Address == correo;
            }
            catch {
                return false;
            }
        }

        public static bool EsContrasenaValida(string contrasena)
        {
            return contrasena.Length >= 6 &&
                   contrasena.Any(char.IsUpper) &&
                   contrasena.Any(char.IsLower) &&
                   contrasena.Any(char.IsDigit);
        }
    }
}
