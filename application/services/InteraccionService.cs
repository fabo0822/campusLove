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

        public async Task<bool> RegistrarInteraccion(int usuarioId, int objetivoId, bool leGusto)
        {
            // Si es un dislike, no necesitamos verificar límites de likes
            if (!leGusto)
            {
                await RegistrarInteraccionSinVerificar(usuarioId, objetivoId, leGusto);
                return true;
            }
            
            // Verificar límite de likes diarios
            bool permiteInteraccion = await VerificarLikesDisponibles(usuarioId);
            if (!permiteInteraccion)
            {
                return false; // No tiene likes disponibles
            }
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                
                // Iniciar una transacción para asegurar que ambas operaciones se realicen juntas
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Registrar la interacción
                        var cmdInteraccion = new MySqlCommand(
                            @"INSERT INTO interacciones (usuario_id, objetivo_usuario_id, le_gusto, fecha_interaccion) 
                            VALUES (@usuarioId, @objetivoId, @leGusto, @fecha)", 
                            (MySqlConnection)conn);
                        
                        cmdInteraccion.Parameters.AddWithValue("@usuarioId", usuarioId);
                        cmdInteraccion.Parameters.AddWithValue("@objetivoId", objetivoId);
                        cmdInteraccion.Parameters.AddWithValue("@leGusto", leGusto);
                        cmdInteraccion.Parameters.AddWithValue("@fecha", DateTime.Now);
                        cmdInteraccion.Transaction = (MySqlTransaction)transaction;
                        
                        await cmdInteraccion.ExecuteNonQueryAsync();
                        
                        // Incrementar el contador de likes diarios del usuario
                        var cmdIncrementarLikes = new MySqlCommand(
                            @"UPDATE usuarios 
                            SET likes_diarios = likes_diarios + 1 
                            WHERE id = @usuarioId", 
                            (MySqlConnection)conn);
                        
                        cmdIncrementarLikes.Parameters.AddWithValue("@usuarioId", usuarioId);
                        cmdIncrementarLikes.Transaction = (MySqlTransaction)transaction;
                        
                        await cmdIncrementarLikes.ExecuteNonQueryAsync();
                        
                        // Confirmar la transacción
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        // Si hay un error, hacer rollback de la transacción
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        
        private async Task RegistrarInteraccionSinVerificar(int usuarioId, int objetivoId, bool leGusto)
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
        
        public async Task<bool> VerificarLikesDisponibles(int usuarioId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT likes_diarios, max_likes_diarios FROM usuarios WHERE id = @usuarioId", 
                    (MySqlConnection)conn);
                
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        int likesActuales = Convert.ToInt32(reader["likes_diarios"]);
                        int maxLikesDiarios = Convert.ToInt32(reader["max_likes_diarios"]);
                        
                        return likesActuales < maxLikesDiarios;
                    }
                    return false; // Si no encuentra el usuario
                }
            }
        }
        
        public async Task<(int Actuales, int Maximos)> ObtenerLikesInfo(int usuarioId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT likes_diarios, max_likes_diarios FROM usuarios WHERE id = @usuarioId", 
                    (MySqlConnection)conn);
                
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        int likesActuales = Convert.ToInt32(reader["likes_diarios"]);
                        int maxLikesDiarios = Convert.ToInt32(reader["max_likes_diarios"]);
                        
                        return (likesActuales, maxLikesDiarios);
                    }
                    return (0, 0); // Si no encuentra el usuario
                }
            }
        }
        
        public async Task ResetearLikesDiarios()
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("UPDATE usuarios SET likes_diarios = 0", (MySqlConnection)conn);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task VerificarCoincidencia(int usuarioId, int objetivoId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                
                // Primero verificamos si ya existe una coincidencia para evitar duplicados
                var cmdVerificarExistente = new MySqlCommand(
                    @"SELECT COUNT(*) FROM coincidencias 
                    WHERE (usuario1_id = @usuarioId AND usuario2_id = @objetivoId)
                    OR (usuario1_id = @objetivoId AND usuario2_id = @usuarioId)", 
                    (MySqlConnection)conn);

                cmdVerificarExistente.Parameters.AddWithValue("@usuarioId", usuarioId);
                cmdVerificarExistente.Parameters.AddWithValue("@objetivoId", objetivoId);

                var coincidenciaExistente = Convert.ToInt32(await cmdVerificarExistente.ExecuteScalarAsync());
                if (coincidenciaExistente > 0)
                {
                    return; // Ya existe una coincidencia, no hacemos nada
                }

                // Verificamos si hay un like mutuo
                var cmdVerificarLike = new MySqlCommand(
                    @"SELECT COUNT(*) FROM interacciones 
                    WHERE usuario_id = @objetivoId 
                    AND objetivo_usuario_id = @usuarioId 
                    AND le_gusto = true", 
                    (MySqlConnection)conn);

                cmdVerificarLike.Parameters.AddWithValue("@usuarioId", usuarioId);
                cmdVerificarLike.Parameters.AddWithValue("@objetivoId", objetivoId);

                var coincidencia = Convert.ToInt32(await cmdVerificarLike.ExecuteScalarAsync());
                if (coincidencia > 0)
                {
                    await RegistrarCoincidencia(usuarioId, objetivoId);
                    
                    // Actualizamos las estadísticas de ambos usuarios
                    var cmdActualizarEstadisticas = new MySqlCommand(
                        @"UPDATE estadisticas 
                        SET coincidencias_totales = coincidencias_totales + 1 
                        WHERE usuario_id IN (@usuarioId, @objetivoId)", 
                        (MySqlConnection)conn);

                    cmdActualizarEstadisticas.Parameters.AddWithValue("@usuarioId", usuarioId);
                    cmdActualizarEstadisticas.Parameters.AddWithValue("@objetivoId", objetivoId);
                    
                    await cmdActualizarEstadisticas.ExecuteNonQueryAsync();
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