using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using campusLove.infraestructure.mysql;
using campusLove.domain.models;
using campusLove.domain.ports;
using System.Globalization;

namespace campusLove.application.services
{
    /// <summary>
    /// Implementación del puerto primario IEstadisticaService.
    /// Actúa como un adaptador primario en la arquitectura hexagonal.
    /// </summary>
    public class EstadisticaService : IEstadisticaService
    {
        private readonly MySqlDbFactory _dbFactory;
        private readonly CultureInfo _culture;
        private const int LIMITE_TOP_USUARIOS = 10;

        public EstadisticaService(MySqlDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
            _culture = new CultureInfo("es-ES");
        }

        public async Task<IEnumerable<EstadisticaUsuario>> ObtenerUsuariosConMasLikes()
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(@"
                    SELECT 
                        u.id,
                        u.nombre,
                        COUNT(DISTINCT CASE WHEN i.le_gusto = 1 THEN i.usuario_id END) as likes_recibidos,
                        COUNT(DISTINCT c.id) as total_matches
                    FROM usuarios u
                    LEFT JOIN interacciones i ON u.id = i.objetivo_usuario_id
                    LEFT JOIN coincidencias c ON (u.id = c.usuario1_id OR u.id = c.usuario2_id)
                    GROUP BY u.id, u.nombre
                    ORDER BY likes_recibidos DESC
                    LIMIT @limite", (MySqlConnection)conn);

                cmd.Parameters.AddWithValue("@limite", LIMITE_TOP_USUARIOS);

                var usuarios = new List<EstadisticaUsuario>();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        usuarios.Add(new EstadisticaUsuario
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                            LikesRecibidos = reader.GetInt32(reader.GetOrdinal("likes_recibidos")),
                            Matches = reader.GetInt32(reader.GetOrdinal("total_matches"))
                        });
                    }
                }

                return usuarios
                    .OrderByDescending(u => u.LikesRecibidos)
                    .ThenByDescending(u => u.Matches)
                    .Take(LIMITE_TOP_USUARIOS);
            }
        }

        public async Task<Dictionary<string, int>> ObtenerEstadisticasGenerales()
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(@"
                    SELECT 
                        (SELECT COUNT(*) FROM usuarios) as total_usuarios,
                        (SELECT COUNT(*) FROM interacciones WHERE le_gusto = 1) as total_likes,
                        (SELECT COUNT(*) FROM coincidencias) as total_matches,
                        (SELECT COUNT(*) FROM usuarios WHERE genero = 1) as total_hombres,
                        (SELECT COUNT(*) FROM usuarios WHERE genero = 2) as total_mujeres
                    FROM dual", (MySqlConnection)conn);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Dictionary<string, int>
                        {
                            ["TotalUsuarios"] = reader.GetInt32(reader.GetOrdinal("total_usuarios")),
                            ["TotalLikes"] = reader.GetInt32(reader.GetOrdinal("total_likes")),
                            ["TotalMatches"] = reader.GetInt32(reader.GetOrdinal("total_matches")),
                            ["TotalHombres"] = reader.GetInt32(reader.GetOrdinal("total_hombres")),
                            ["TotalMujeres"] = reader.GetInt32(reader.GetOrdinal("total_mujeres"))
                        };
                    }
                }
            }
            return new Dictionary<string, int>();
        }

        public async Task MostrarEstadisticas()
        {
            var topUsuarios = await ObtenerUsuariosConMasLikes();
            var estadisticasGenerales = await ObtenerEstadisticasGenerales();
            
            Console.WriteLine("\n=== Estadísticas Generales ===");
            Console.WriteLine($"Total Usuarios: {estadisticasGenerales["TotalUsuarios"]:N0}");
            Console.WriteLine($"Total Likes: {estadisticasGenerales["TotalLikes"]:N0}");
            Console.WriteLine($"Total Matches: {estadisticasGenerales["TotalMatches"]:N0}");
            Console.WriteLine($"Usuarios Masculinos: {estadisticasGenerales["TotalHombres"]:N0}");
            Console.WriteLine($"Usuarios Femeninos: {estadisticasGenerales["TotalMujeres"]:N0}");
            
            Console.WriteLine("\n=== Top Usuarios con más Likes ===");
            foreach (var usuario in topUsuarios)
            {
                Console.WriteLine($"\nUsuario: {usuario.Nombre}");
                Console.WriteLine($"Likes recibidos: {usuario.LikesRecibidos:N0}");
                Console.WriteLine($"Matches totales: {usuario.Matches:N0}");
                Console.WriteLine($"Porcentaje de éxito: {usuario.PorcentajeExito:N1}%");
                Console.WriteLine("----------------------------");
            }
        }

        public async Task ActualizarEstadisticas(int usuarioId)
        {
            // La actualización ahora es en tiempo real a través de las consultas
            await Task.CompletedTask;
        }
    }
} 