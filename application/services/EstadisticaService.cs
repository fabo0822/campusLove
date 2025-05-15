using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using campusLove.domain.entities;
using campusLove.infraestructure.mysql;

namespace campusLove.application.services
{
    public class EstadisticaService
    {
        private readonly MySqlDbFactory _dbFactory;

        public EstadisticaService(MySqlDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task MostrarEstadisticas()
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT u.nombre, e.likes_recibidos, e.coincidencias_totales 
                    FROM estadisticas e 
                    JOIN usuarios u ON e.usuario_id = u.id 
                    ORDER BY e.likes_recibidos DESC", 
                    (MySqlConnection)conn);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Console.WriteLine($"Usuario: {reader["nombre"]}, " +
                            $"Likes Recibidos: {reader["likes_recibidos"]}, " +
                            $"Coincidencias: {reader["coincidencias_totales"]}");
                    }
                }
            }
        }

        public async Task ActualizarEstadisticas(int usuarioId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"UPDATE estadisticas e 
                    SET e.likes_recibidos = (
                        SELECT COUNT(*) 
                        FROM interacciones i 
                        WHERE i.objetivo_usuario_id = e.usuario_id 
                        AND i.le_gusto = true
                    ),
                    e.coincidencias_totales = (
                        SELECT COUNT(*) 
                        FROM coincidencias c 
                        WHERE c.usuario1_id = e.usuario_id 
                        OR c.usuario2_id = e.usuario_id
                    )
                    WHERE e.usuario_id = @usuarioId", 
                    (MySqlConnection)conn);

                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
} 