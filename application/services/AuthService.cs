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

        public async Task<(bool success, int userId)> Login(string correo, string contrasena)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT l.usuario_id 
                    FROM login l 
                    WHERE l.correo = @correo 
                    AND l.contrasena = @contrasena", 
                    (MySqlConnection)conn);

                cmd.Parameters.AddWithValue("@correo", correo);
                cmd.Parameters.AddWithValue("@contrasena", contrasena);

                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    return (true, Convert.ToInt32(result));
                }
                return (false, -1);
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