using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using campusLove.domain.entities;
using campusLove.infraestructure.mysql;

namespace campusLove.application.services
{
    public class AuthService
    {
        private readonly MySqlDbFactory _dbFactory;

        public AuthService(MySqlDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<(bool success, int userId, bool isAdmin)> Login(string correo, string contrasena)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT l.usuario_id, l.es_admin 
                    FROM login l 
                    WHERE l.correo = @correo 
                    AND l.contrasena = @contrasena", 
                    (MySqlConnection)conn);

                cmd.Parameters.AddWithValue("@correo", correo);
                cmd.Parameters.AddWithValue("@contrasena", contrasena);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        int userId = reader.GetInt32(0);
                        bool isAdmin = reader.GetBoolean(1);
                        return (true, userId, isAdmin);
                    }
                }
                return (false, -1, false);
            }
        }

        public async Task<bool> RegistrarLogin(int usuarioId, string correo, string contrasena)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"INSERT INTO login (usuario_id, correo, contrasena) 
                    VALUES (@usuarioId, @correo, @contrasena)", 
                    (MySqlConnection)conn);

                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@correo", correo);
                cmd.Parameters.AddWithValue("@contrasena", contrasena);

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                    return true;
                }
                catch (MySqlException)
                {
                    return false;
                }
            }
        }
    }
} 