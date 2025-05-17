using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using campusLove.domain.entities;
using campusLove.infraestructure.mysql;

namespace campusLove.application.services
{
    public class UsuarioService
    {
        private readonly MySqlDbFactory _dbFactory;

        public UsuarioService(MySqlDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task RegistrarUsuario(string nombre, int edad, int genero, string intereses, 
            string carrera, string frase, int ciudadId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"INSERT INTO usuarios (nombre, edad, genero, intereses, carrera, frase, 
                    likes_diarios, max_likes_diarios, ciudad_id) 
                    VALUES (@nombre, @edad, @genero, @intereses, @carrera, @frase, 0, 10, @ciudadId)", 
                    (MySqlConnection)conn);

                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.Parameters.AddWithValue("@edad", edad);
                cmd.Parameters.AddWithValue("@genero", genero);
                cmd.Parameters.AddWithValue("@intereses", intereses);
                cmd.Parameters.AddWithValue("@carrera", carrera);
                cmd.Parameters.AddWithValue("@frase", frase);
                cmd.Parameters.AddWithValue("@ciudadId", ciudadId);
                
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task MostrarPerfiles(int currentUserId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT id, nombre, edad, carrera, frase 
                    FROM usuarios 
                    WHERE id != @currentUserId", 
                    (MySqlConnection)conn);
                
                cmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Console.WriteLine($"ID: {reader["id"]}, " +
                            $"Nombre: {reader["nombre"]}, " +
                            $"Edad: {reader["edad"]}, " +
                            $"Carrera: {reader["carrera"]}, " +
                            $"Frase: {reader["frase"]}");
                    }
                }
            }
        }
    }
} 