using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using campusLove.domain.entities;
using campusLove.infraestructure.mysql;

namespace campusLove.application.services
{
    public class InteraccionService
    {
        private readonly MySqlDbFactory _dbFactory;

        public InteraccionService(MySqlDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task RegistrarInteraccion(int usuarioId, int objetivoId, bool leGusto)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"INSERT INTO interacciones (usuario_id, objetivo_usuario_id, le_gusto, fecha_interaccion) 
                    VALUES (@usuarioId, @objetivoId, @leGusto, @fecha)", 
                    (MySqlConnection)conn);

                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@objetivoId", objetivoId);
                cmd.Parameters.AddWithValue("@leGusto", leGusto);
                cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task VerificarCoincidencia(int usuarioId, int objetivoId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT COUNT(*) FROM interacciones 
                    WHERE usuario_id = @objetivoId 
                    AND objetivo_usuario_id = @usuarioId 
                    AND le_gusto = true", 
                    (MySqlConnection)conn);

                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@objetivoId", objetivoId);

                var coincidencia = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                if (coincidencia > 0)
                {
                    await RegistrarCoincidencia(usuarioId, objetivoId);
                }
            }
        }

        private async Task RegistrarCoincidencia(int usuario1Id, int usuario2Id)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"INSERT INTO coincidencias (usuario1_id, usuario2_id, fecha_coincidencia) 
                    VALUES (@usuario1Id, @usuario2Id, @fecha)", 
                    (MySqlConnection)conn);

                cmd.Parameters.AddWithValue("@usuario1Id", usuario1Id);
                cmd.Parameters.AddWithValue("@usuario2Id", usuario2Id);
                cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
} 